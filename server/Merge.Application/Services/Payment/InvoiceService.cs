using AutoMapper;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Payment;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Payment;
using Merge.Application.DTOs.User;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using Invoice = Merge.Domain.Modules.Ordering.Invoice;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IInvoiceRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Ordering.Invoice>;
using IOrderRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Ordering.Order>;

namespace Merge.Application.Services.Payment;

public class InvoiceService(IInvoiceRepository invoiceRepository, IOrderRepository orderRepository, IDbContext context, IMapper mapper, IUnitOfWork unitOfWork, ILogger<InvoiceService> logger, IOptions<PaymentSettings> paymentSettings) : IInvoiceService
{
    private readonly PaymentSettings paymentConfig = paymentSettings.Value;

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<InvoiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {

        var invoice = await context.Set<Invoice>()
            .AsNoTracking()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (invoice == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber, BillingAddress, Items AutoMapper'da zaten map ediliyor
        return mapper.Map<InvoiceDto>(invoice);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<InvoiceDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var invoice = await context.Set<Invoice>()
            .AsNoTracking()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.OrderId == orderId, cancellationToken);

        if (invoice == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber, BillingAddress, Items AutoMapper'da zaten map ediliyor
        return mapper.Map<InvoiceDto>(invoice);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<InvoiceDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes with nested ThenInclude)
        var query = context.Set<Invoice>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .Where(i => i.Order.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        // Not: OrderNumber, BillingAddress, Items AutoMapper'da zaten map ediliyor
        var dtos = mapper.Map<IEnumerable<InvoiceDto>>(invoices);

        return new PagedResult<InvoiceDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<InvoiceDto> GenerateInvoiceAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Invoice oluşturuluyor. OrderId: {OrderId}",
            orderId);

        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes with ThenInclude)
        var order = await context.Set<OrderEntity>()
            .AsSplitQuery()
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken); // ✅ Global Query Filter handles !o.IsDeleted

        if (order == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        if (order.PaymentStatus != PaymentStatus.Completed)
        {
            throw new BusinessException("Sadece ödenmiş siparişler için fatura oluşturulabilir.");
        }

        // Mevcut fatura var mı kontrol et
        // ✅ PERFORMANCE: Global Query Filter automatically filters !i.IsDeleted
        var existingInvoice = await context.Set<Invoice>()
            .FirstOrDefaultAsync(i => i.OrderId == orderId, cancellationToken);

        if (existingInvoice != null)
        {
            return await GetByIdAsync(existingInvoice.Id, cancellationToken) ?? throw new BusinessException("Fatura oluşturulamadı.");
        }

        var invoiceNumber = GenerateInvoiceNumber();

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
        var subTotalMoney = new Money(order.SubTotal);
        var taxMoney = new Money(order.Tax);
        var shippingCostMoney = new Money(order.ShippingCost);
        var discountMoney = new Money(order.CouponDiscount ?? 0);
        var totalAmountMoney = new Money(order.TotalAmount);
        var invoice = Invoice.Create(
            orderId: orderId,
            invoiceNumber: invoiceNumber,
            invoiceDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow.AddDays(paymentConfig.InvoiceDueDays), // ✅ BOLUM 12.0: Magic number config'den
            subTotal: subTotalMoney,
            tax: taxMoney,
            shippingCost: shippingCostMoney,
            discount: discountMoney,
            totalAmount: totalAmountMoney);

        invoice = await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        invoice = await context.Set<Invoice>()
            .AsNoTracking()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Invoice oluşturuldu. InvoiceId: {InvoiceId}, InvoiceNumber: {InvoiceNumber}, OrderId: {OrderId}",
            invoice!.Id, invoice.InvoiceNumber, orderId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber, BillingAddress, Items AutoMapper'da zaten map ediliyor
        return mapper.Map<InvoiceDto>(invoice);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SendInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        invoice.MarkAsSent();
        await invoiceRepository.UpdateAsync(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Email gönderilebilir (EmailService ile)
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<string> GenerateInvoicePdfAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        // Burada PDF oluşturma kütüphanesi kullanılacak (iTextSharp, QuestPDF, vb.)
        // Şimdilik sadece placeholder URL döndürüyoruz
        
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
        {
            throw new NotFoundException("Fatura", invoiceId);
        }

        // PDF oluşturma işlemi burada yapılacak
        // Örnek: var pdfBytes = await GeneratePdfBytes(invoice);
        // var pdfUrl = await UploadPdfToStorage(pdfBytes, invoice.InvoiceNumber);
        
        var pdfUrl = $"/invoices/{invoice.InvoiceNumber}.pdf";
        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        invoice.SetPdfUrl(pdfUrl);
        await invoiceRepository.UpdateAsync(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return pdfUrl;
    }

    private string GenerateInvoiceNumber()
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month.ToString("D2");
        var day = DateTime.UtcNow.Day.ToString("D2");
        var random = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        return $"INV-{year}{month}{day}-{random}";
    }
}


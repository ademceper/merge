using AutoMapper;
using PaymentEntity = Merge.Domain.Entities.Payment;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Payment;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Payment;
using Merge.Application.DTOs.User;


namespace Merge.Application.Services.Payment;

public class InvoiceService : IInvoiceService
{
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<OrderEntity> _orderRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IRepository<Invoice> invoiceRepository,
        IRepository<OrderEntity> orderRepository,
        ApplicationDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<InvoiceService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _orderRepository = orderRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<InvoiceDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber, BillingAddress, Items AutoMapper'da zaten map ediliyor
        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<InvoiceDto?> GetByOrderIdAsync(Guid orderId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.OrderId == orderId);

        if (invoice == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber, BillingAddress, Items AutoMapper'da zaten map ediliyor
        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<IEnumerable<InvoiceDto>> GetByUserIdAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .Where(i => i.Order.UserId == userId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper kullan
        // Not: OrderNumber, BillingAddress, Items AutoMapper'da zaten map ediliyor
        return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
    }

    public async Task<InvoiceDto> GenerateInvoiceAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId); // ✅ Global Query Filter handles !o.IsDeleted

        if (order == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        if (order.PaymentStatus != "Paid")
        {
            throw new BusinessException("Sadece ödenmiş siparişler için fatura oluşturulabilir.");
        }

        // Mevcut fatura var mı kontrol et
        // ✅ PERFORMANCE: Global Query Filter automatically filters !i.IsDeleted
        var existingInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.OrderId == orderId);

        if (existingInvoice != null)
        {
            return await GetByIdAsync(existingInvoice.Id) ?? throw new BusinessException("Fatura oluşturulamadı.");
        }

        var invoiceNumber = GenerateInvoiceNumber();

        var invoice = new Invoice
        {
            OrderId = orderId,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30), // 30 gün vade
            SubTotal = order.SubTotal,
            Tax = order.Tax,
            ShippingCost = order.ShippingCost,
            Discount = order.CouponDiscount ?? 0,
            TotalAmount = order.TotalAmount,
            Status = "Draft"
        };

        invoice = await _invoiceRepository.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
        invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Order)
                .ThenInclude(o => o.Address)
            .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(i => i.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // Not: OrderNumber, BillingAddress, Items AutoMapper'da zaten map ediliyor
        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<bool> SendInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
        {
            return false;
        }

        invoice.Status = "Sent";
        await _invoiceRepository.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        // Email gönderilebilir (EmailService ile)
        return true;
    }

    public async Task<string> GenerateInvoicePdfAsync(Guid invoiceId)
    {
        // Burada PDF oluşturma kütüphanesi kullanılacak (iTextSharp, QuestPDF, vb.)
        // Şimdilik sadece placeholder URL döndürüyoruz
        
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
        {
            throw new NotFoundException("Fatura", invoiceId);
        }

        // PDF oluşturma işlemi burada yapılacak
        // Örnek: var pdfBytes = await GeneratePdfBytes(invoice);
        // var pdfUrl = await UploadPdfToStorage(pdfBytes, invoice.InvoiceNumber);
        
        var pdfUrl = $"/invoices/{invoice.InvoiceNumber}.pdf";
        invoice.PdfUrl = pdfUrl;
        await _invoiceRepository.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

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


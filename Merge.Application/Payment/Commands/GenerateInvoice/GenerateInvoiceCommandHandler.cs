using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.GenerateInvoice;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, InvoiceDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GenerateInvoiceCommandHandler> _logger;
    private readonly PaymentSettings _paymentSettings;

    public GenerateInvoiceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GenerateInvoiceCommandHandler> logger,
        IOptions<PaymentSettings> paymentSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _paymentSettings = paymentSettings.Value;
    }

    public async Task<InvoiceDto> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating invoice. OrderId: {OrderId}", request.OrderId);

        // CRITICAL: Transaction baslat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
            var order = await _context.Set<OrderEntity>()
                .AsSplitQuery()
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
                throw new NotFoundException("Sipariş", request.OrderId);
            }

            if (order.PaymentStatus != PaymentStatus.Completed)
            {
                _logger.LogWarning("Order payment status is not completed. OrderId: {OrderId}, Status: {Status}",
                    request.OrderId, order.PaymentStatus);
                throw new BusinessException("Sadece ödenmiş siparişler için fatura oluşturulabilir.");
            }

            // Check if invoice already exists
            var existingInvoice = await _context.Set<Invoice>()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.OrderId == request.OrderId, cancellationToken);

            if (existingInvoice != null)
            {
                _logger.LogInformation("Invoice already exists. InvoiceId: {InvoiceId}, OrderId: {OrderId}",
                    existingInvoice.Id, request.OrderId);
                // Reload with includes
                // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
                var reloadedInvoice = await _context.Set<Invoice>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Address)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.OrderItems)
                            .ThenInclude(oi => oi.Product)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.User)
                    .FirstOrDefaultAsync(i => i.Id == existingInvoice.Id, cancellationToken);

                return _mapper.Map<InvoiceDto>(reloadedInvoice!);
            }

            var invoiceNumber = GenerateInvoiceNumber();
            var dueDate = DateTime.UtcNow.AddDays(_paymentSettings.InvoiceDueDays);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var invoice = Invoice.Create(
                request.OrderId,
                invoiceNumber,
                DateTime.UtcNow,
                dueDate,
                order.SubTotal,
                order.Tax,
                order.ShippingCost,
                order.CouponDiscount ?? 0,
                order.TotalAmount);

            await _context.Set<Invoice>().AddAsync(invoice, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
            // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
            invoice = await _context.Set<Invoice>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(i => i.Order)
                    .ThenInclude(o => o.Address)
                .Include(i => i.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .Include(i => i.Order)
                    .ThenInclude(o => o.User)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Invoice generated successfully. InvoiceId: {InvoiceId}, InvoiceNumber: {InvoiceNumber}, OrderId: {OrderId}",
                invoice!.Id, invoiceNumber, request.OrderId);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<InvoiceDto>(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice. OrderId: {OrderId}", request.OrderId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
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

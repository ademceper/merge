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
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.GenerateInvoice;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GenerateInvoiceCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<GenerateInvoiceCommandHandler> logger, IOptions<PaymentSettings> paymentSettings) : IRequestHandler<GenerateInvoiceCommand, InvoiceDto>
{
    private readonly PaymentSettings paymentConfig = paymentSettings.Value;

    public async Task<InvoiceDto> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating invoice. OrderId: {OrderId}", request.OrderId);

        // CRITICAL: Transaction baslat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = await context.Set<OrderEntity>()
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
                throw new NotFoundException("Sipariş", request.OrderId);
            }

            if (order.PaymentStatus != PaymentStatus.Completed)
            {
                logger.LogWarning("Order payment status is not completed. OrderId: {OrderId}, Status: {Status}",
                    request.OrderId, order.PaymentStatus);
                throw new BusinessException("Sadece ödenmiş siparişler için fatura oluşturulabilir.");
            }

            // Check if invoice already exists
            var existingInvoice = await context.Set<Invoice>()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.OrderId == request.OrderId, cancellationToken);

            if (existingInvoice != null)
            {
                logger.LogInformation("Invoice already exists. InvoiceId: {InvoiceId}, OrderId: {OrderId}",
                    existingInvoice.Id, request.OrderId);

                var reloadedInvoice = await context.Set<Invoice>()
                    .AsNoTracking()
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Address)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.OrderItems)
                            .ThenInclude(oi => oi.Product)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.User)
                    .FirstOrDefaultAsync(i => i.Id == existingInvoice.Id, cancellationToken);

                return mapper.Map<InvoiceDto>(reloadedInvoice!);
            }

            var invoiceNumber = GenerateInvoiceNumber();
            var dueDate = DateTime.UtcNow.AddDays(paymentConfig.InvoiceDueDays);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
            var subTotalMoney = new Money(order.SubTotal);
            var taxMoney = new Money(order.Tax);
            var shippingCostMoney = new Money(order.ShippingCost);
            var discountMoney = new Money(order.CouponDiscount ?? 0);
            var totalAmountMoney = new Money(order.TotalAmount);
            var invoice = Invoice.Create(
                request.OrderId,
                invoiceNumber,
                DateTime.UtcNow,
                dueDate,
                subTotalMoney,
                taxMoney,
                shippingCostMoney,
                discountMoney,
                totalAmountMoney);

            await context.Set<Invoice>().AddAsync(invoice, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
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

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Invoice generated successfully. InvoiceId: {InvoiceId}, InvoiceNumber: {InvoiceNumber}, OrderId: {OrderId}",
                invoice!.Id, invoiceNumber, request.OrderId);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return mapper.Map<InvoiceDto>(invoice);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating invoice. OrderId: {OrderId}", request.OrderId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
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

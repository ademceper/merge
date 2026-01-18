using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.RefundPayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class RefundPaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<RefundPaymentCommandHandler> logger) : IRequestHandler<RefundPaymentCommand, PaymentDto>
{

    public async Task<PaymentDto> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Initiating refund for payment ID: {PaymentId}, Refund Amount: {RefundAmount}",
            request.PaymentId, request.Amount?.ToString() ?? "Full");

        // CRITICAL: Transaction baslat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var payment = await context.Set<PaymentEntity>()
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

            if (payment is null)
            {
                logger.LogWarning("Payment not found with ID: {PaymentId}", request.PaymentId);
                throw new NotFoundException("Odeme kaydi", request.PaymentId);
            }

            if (payment.Status != PaymentStatus.Completed)
            {
                logger.LogWarning("Payment {PaymentId} cannot be refunded. Status: {Status}", request.PaymentId, payment.Status);
                throw new BusinessException("Sadece tamamlanmis odemeler iade edilebilir.");
            }

            var refundAmount = request.Amount ?? payment.Amount;

            if (refundAmount > payment.Amount)
            {
                logger.LogWarning(
                    "Refund amount {RefundAmount} exceeds payment amount {PaymentAmount} for payment {PaymentId}",
                    refundAmount, payment.Amount, request.PaymentId);
                throw new ValidationException("Iade tutari odeme tutarindan fazla olamaz.");
            }

            // BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Burada gercek payment gateway refund islemi yapilacak
            var refundMoney = new Money(refundAmount);
            var isFullRefund = refundAmount == payment.Amount;

            if (isFullRefund)
            {
                payment.Refund();
            }
            else
            {
                payment.PartiallyRefund(refundMoney);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Updated payment {PaymentId} status to {Status}", request.PaymentId, payment.Status);

            // BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Order payment status'unu guncelle
            var order = await context.Set<OrderEntity>()
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId, cancellationToken);

            if (order is not null)
            {
                order.SetPaymentStatus(isFullRefund ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Updated order {OrderId} payment status to {Status}", payment.OrderId, order.PaymentStatus);
            }

            // PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            payment = await context.Set<PaymentEntity>()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == payment.Id, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Payment refunded successfully. PaymentId: {PaymentId}, RefundAmount: {RefundAmount}, IsFullRefund: {IsFullRefund}",
                request.PaymentId, refundAmount, isFullRefund);

            // ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return mapper.Map<PaymentDto>(payment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refunding payment with ID: {PaymentId}, Refund Amount: {RefundAmount}",
                request.PaymentId, request.Amount);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

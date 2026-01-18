using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Text.Json;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Enums;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.ProcessPayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class ProcessPaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProcessPaymentCommandHandler> logger) : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{

    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing payment with ID: {PaymentId}, TransactionId: {TransactionId}",
            request.PaymentId, request.TransactionId);

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

            if (payment.Status == PaymentStatus.Completed)
            {
                logger.LogWarning("Payment {PaymentId} is already completed", request.PaymentId);
                throw new BusinessException("Bu odeme zaten tamamlanmis.");
            }

            // BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Burada gercek payment gateway entegrasyonu yapilacak
            payment.Process();
            payment.Complete(request.TransactionId, request.PaymentReference);

            if (request.Metadata is not null)
            {
                payment.SetMetadata(JsonSerializer.Serialize(request.Metadata));
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Order payment status'unu guncelle
            var order = await context.Set<OrderEntity>()
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId, cancellationToken);

            if (order is not null)
            {
                order.SetPaymentStatus(PaymentStatus.Completed);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Updated order {OrderId} payment status to Completed", payment.OrderId);
            }

            // PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            payment = await context.Set<PaymentEntity>()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == payment.Id, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Payment processed successfully. PaymentId: {PaymentId}, TransactionId: {TransactionId}",
                request.PaymentId, request.TransactionId);

            // ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return mapper.Map<PaymentDto>(payment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing payment with ID: {PaymentId}", request.PaymentId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

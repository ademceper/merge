using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.ProcessPayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ProcessPaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ProcessPaymentCommandHandler> logger) : IRequestHandler<ProcessPaymentCommand, bool>
{

    public async Task<bool> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Processing subscription payment. PaymentId: {PaymentId}, TransactionId: {TransactionId}",
            request.PaymentId, request.TransactionId);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var payment = await context.Set<SubscriptionPayment>()
            .Include(p => p.UserSubscription)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("Ödeme", request.PaymentId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        payment.MarkAsCompleted(request.TransactionId);

        // Update subscription if needed
        if (payment.UserSubscription != null && payment.UserSubscription.Status == SubscriptionStatus.Trial)
        {
            payment.UserSubscription.Activate();
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Subscription payment processed successfully. PaymentId: {PaymentId}", payment.Id);

        return true;
    }
}

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

public class ProcessPaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ProcessPaymentCommandHandler> logger) : IRequestHandler<ProcessPaymentCommand, bool>
{

    public async Task<bool> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing subscription payment. PaymentId: {PaymentId}, TransactionId: {TransactionId}",
            request.PaymentId, request.TransactionId);

        var payment = await context.Set<SubscriptionPayment>()
            .Include(p => p.UserSubscription)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("Ödeme", request.PaymentId);
        }

        payment.MarkAsCompleted(request.TransactionId);

        // Update subscription if needed
        if (payment.UserSubscription != null && payment.UserSubscription.Status == SubscriptionStatus.Trial)
        {
            payment.UserSubscription.Activate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Subscription payment processed successfully. PaymentId: {PaymentId}", payment.Id);

        return true;
    }
}

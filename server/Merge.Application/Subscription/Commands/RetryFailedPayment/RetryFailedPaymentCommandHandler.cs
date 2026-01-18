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

namespace Merge.Application.Subscription.Commands.RetryFailedPayment;

public class RetryFailedPaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RetryFailedPaymentCommandHandler> logger) : IRequestHandler<RetryFailedPaymentCommand, bool>
{

    public async Task<bool> Handle(RetryFailedPaymentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrying failed payment. PaymentId: {PaymentId}", request.PaymentId);

        var payment = await context.Set<SubscriptionPayment>()
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.PaymentStatus == PaymentStatus.Failed, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("Başarısız ödeme", request.PaymentId);
        }

        payment.Retry();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Failed payment retry initiated. PaymentId: {PaymentId}, RetryCount: {RetryCount}",
            payment.Id, payment.RetryCount);

        return true;
    }
}

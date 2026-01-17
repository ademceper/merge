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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RetryFailedPaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RetryFailedPaymentCommandHandler> logger) : IRequestHandler<RetryFailedPaymentCommand, bool>
{

    public async Task<bool> Handle(RetryFailedPaymentCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Retrying failed payment. PaymentId: {PaymentId}", request.PaymentId);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var payment = await context.Set<SubscriptionPayment>()
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.PaymentStatus == PaymentStatus.Failed, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("Başarısız ödeme", request.PaymentId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        payment.Retry();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Failed payment retry initiated. PaymentId: {PaymentId}, RetryCount: {RetryCount}",
            payment.Id, payment.RetryCount);

        return true;
    }
}

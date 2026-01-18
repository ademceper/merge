using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Queries.VerifyPayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class VerifyPaymentQueryHandler(IDbContext context, ILogger<VerifyPaymentQueryHandler> logger) : IRequestHandler<VerifyPaymentQuery, bool>
{

    public async Task<bool> Handle(VerifyPaymentQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Verifying payment with transaction ID: {TransactionId}", request.TransactionId);

        var payment = await context.Set<PaymentEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TransactionId == request.TransactionId, cancellationToken);

        if (payment == null)
        {
            logger.LogWarning("Payment not found with transaction ID: {TransactionId}", request.TransactionId);
            return false;
        }

        // Burada payment gateway'den ödeme durumu sorgulanacak
        // Şimdilik sadece payment kaydının varlığını ve Completed status'unu kontrol ediyoruz
        var isVerified = payment.Status == PaymentStatus.Completed;

        logger.LogInformation("Payment verification result for transaction ID {TransactionId}: {IsVerified}",
            request.TransactionId, isVerified);

        return isVerified;
    }
}

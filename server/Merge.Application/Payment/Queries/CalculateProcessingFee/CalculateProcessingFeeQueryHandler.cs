using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Queries.CalculateProcessingFee;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class CalculateProcessingFeeQueryHandler(IDbContext context, ILogger<CalculateProcessingFeeQueryHandler> logger) : IRequestHandler<CalculateProcessingFeeQuery, decimal>
{

    public async Task<decimal> Handle(CalculateProcessingFeeQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Calculating processing fee. PaymentMethodId: {PaymentMethodId}, Amount: {Amount}",
            request.PaymentMethodId, request.Amount);

        var paymentMethod = await context.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId && pm.IsActive, cancellationToken);

        if (paymentMethod == null)
        {
            logger.LogWarning("Payment method not found. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            throw new NotFoundException("Ödeme yöntemi", request.PaymentMethodId);
        }

        var fee = paymentMethod.CalculateProcessingFee(request.Amount);

        logger.LogInformation("Processing fee calculated. PaymentMethodId: {PaymentMethodId}, Amount: {Amount}, Fee: {Fee}",
            request.PaymentMethodId, request.Amount, fee);

        return fee;
    }
}

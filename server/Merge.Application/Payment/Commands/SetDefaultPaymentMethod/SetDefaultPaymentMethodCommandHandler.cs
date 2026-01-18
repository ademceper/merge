using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.SetDefaultPaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class SetDefaultPaymentMethodCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<SetDefaultPaymentMethodCommandHandler> logger) : IRequestHandler<SetDefaultPaymentMethodCommand, bool>
{

    public async Task<bool> Handle(SetDefaultPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Setting default payment method. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);

        // CRITICAL: Transaction baslat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var paymentMethod = await context.Set<PaymentMethod>()
                .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId, cancellationToken);

            if (paymentMethod is null)
            {
                logger.LogWarning("Payment method not found. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
                return false;
            }

            // Unset other default methods
            var existingDefault = await context.Set<PaymentMethod>()
                .Where(pm => pm.IsDefault && pm.Id != request.PaymentMethodId)
                .ToListAsync(cancellationToken);

            foreach (var method in existingDefault)
            {
                method.UnsetAsDefault();
            }

            paymentMethod.SetAsDefault();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Default payment method set successfully. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting default payment method. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

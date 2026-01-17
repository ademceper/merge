using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.UpdateUserSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateUserSubscriptionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateUserSubscriptionCommandHandler> logger) : IRequestHandler<UpdateUserSubscriptionCommand, bool>
{

    public async Task<bool> Handle(UpdateUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Updating user subscription. SubscriptionId: {SubscriptionId}", request.Id);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var subscription = await context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.Id, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.AutoRenew.HasValue)
        {
            subscription.UpdateAutoRenew(request.AutoRenew.Value);
        }

        if (!string.IsNullOrEmpty(request.PaymentMethodId))
        {
            subscription.UpdatePaymentMethod(request.PaymentMethodId);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("User subscription updated successfully. SubscriptionId: {SubscriptionId}", subscription.Id);

        return true;
    }
}

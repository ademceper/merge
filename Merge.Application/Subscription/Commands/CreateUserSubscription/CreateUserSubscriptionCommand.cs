using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Commands.CreateUserSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateUserSubscriptionCommand(
    Guid UserId,
    Guid SubscriptionPlanId,
    bool AutoRenew = true,
    string? PaymentMethodId = null) : IRequest<UserSubscriptionDto>;

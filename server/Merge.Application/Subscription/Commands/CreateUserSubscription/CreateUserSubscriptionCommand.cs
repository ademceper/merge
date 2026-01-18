using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Commands.CreateUserSubscription;

public record CreateUserSubscriptionCommand(
    Guid UserId,
    Guid SubscriptionPlanId,
    bool AutoRenew = true,
    string? PaymentMethodId = null) : IRequest<UserSubscriptionDto>;

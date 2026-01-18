using MediatR;

namespace Merge.Application.Subscription.Commands.UpdateUserSubscription;

public record UpdateUserSubscriptionCommand(
    Guid Id,
    bool? AutoRenew = null,
    string? PaymentMethodId = null) : IRequest<bool>;

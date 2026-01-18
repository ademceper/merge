using MediatR;

namespace Merge.Application.Subscription.Commands.CancelUserSubscription;

public record CancelUserSubscriptionCommand(
    Guid SubscriptionId,
    string? Reason = null) : IRequest<bool>;

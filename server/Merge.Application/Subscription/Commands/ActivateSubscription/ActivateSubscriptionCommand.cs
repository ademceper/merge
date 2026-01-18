using MediatR;

namespace Merge.Application.Subscription.Commands.ActivateSubscription;

public record ActivateSubscriptionCommand(Guid SubscriptionId) : IRequest<bool>;

using MediatR;

namespace Merge.Application.Subscription.Commands.SuspendSubscription;

public record SuspendSubscriptionCommand(Guid SubscriptionId) : IRequest<bool>;

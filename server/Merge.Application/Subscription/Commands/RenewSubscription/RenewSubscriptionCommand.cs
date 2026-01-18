using MediatR;

namespace Merge.Application.Subscription.Commands.RenewSubscription;

public record RenewSubscriptionCommand(Guid SubscriptionId) : IRequest<bool>;

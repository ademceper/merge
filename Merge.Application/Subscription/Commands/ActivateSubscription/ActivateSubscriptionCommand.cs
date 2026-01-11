using MediatR;

namespace Merge.Application.Subscription.Commands.ActivateSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ActivateSubscriptionCommand(Guid SubscriptionId) : IRequest<bool>;

using MediatR;

namespace Merge.Application.Subscription.Commands.SuspendSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SuspendSubscriptionCommand(Guid SubscriptionId) : IRequest<bool>;

using MediatR;

namespace Merge.Application.Subscription.Commands.RenewSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RenewSubscriptionCommand(Guid SubscriptionId) : IRequest<bool>;

using MediatR;

namespace Merge.Application.Subscription.Commands.CancelUserSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CancelUserSubscriptionCommand(
    Guid SubscriptionId,
    string? Reason = null) : IRequest<bool>;

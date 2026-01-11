using MediatR;

namespace Merge.Application.Subscription.Commands.UpdateUserSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateUserSubscriptionCommand(
    Guid Id,
    bool? AutoRenew = null,
    string? PaymentMethodId = null) : IRequest<bool>;

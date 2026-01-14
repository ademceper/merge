using MediatR;

namespace Merge.Application.Subscription.Commands.DeleteSubscriptionPlan;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteSubscriptionPlanCommand(Guid Id) : IRequest<bool>;

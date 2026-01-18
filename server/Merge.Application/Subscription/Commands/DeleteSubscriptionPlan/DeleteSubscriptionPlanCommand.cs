using MediatR;

namespace Merge.Application.Subscription.Commands.DeleteSubscriptionPlan;

public record DeleteSubscriptionPlanCommand(Guid Id) : IRequest<bool>;

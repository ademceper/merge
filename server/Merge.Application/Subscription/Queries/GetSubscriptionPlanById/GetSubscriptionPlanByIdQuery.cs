using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetSubscriptionPlanById;

public record GetSubscriptionPlanByIdQuery(Guid Id) : IRequest<SubscriptionPlanDto?>;

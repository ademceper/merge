using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetUserSubscriptionById;

public record GetUserSubscriptionByIdQuery(Guid Id) : IRequest<UserSubscriptionDto?>;

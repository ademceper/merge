using MediatR;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Common;
using Merge.Domain.Enums;

namespace Merge.Application.Subscription.Queries.GetUserSubscriptions;

public record GetUserSubscriptionsQuery(
    Guid UserId,
    SubscriptionStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<UserSubscriptionDto>>;

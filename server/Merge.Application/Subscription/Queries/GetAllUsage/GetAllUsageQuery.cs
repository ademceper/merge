using MediatR;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Common;

namespace Merge.Application.Subscription.Queries.GetAllUsage;

public record GetAllUsageQuery(
    Guid UserSubscriptionId,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<SubscriptionUsageDto>>;

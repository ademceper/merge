using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Analytics.Queries.GetPendingReviews;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPendingReviewsQuery(
    int Page = 1,
    int PageSize = 0
) : IRequest<PagedResult<ReviewDto>>;


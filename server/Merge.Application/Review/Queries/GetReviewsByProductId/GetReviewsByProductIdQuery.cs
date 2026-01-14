using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetReviewsByProductId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetReviewsByProductIdQuery(
    Guid ProductId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ReviewDto>>;

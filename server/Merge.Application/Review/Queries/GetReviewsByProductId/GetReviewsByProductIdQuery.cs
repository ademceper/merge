using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetReviewsByProductId;

public record GetReviewsByProductIdQuery(
    Guid ProductId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ReviewDto>>;

using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetTopReviewedProducts;

public record GetTopReviewedProductsQuery(
    int Limit
) : IRequest<List<TopReviewedProductDto>>;


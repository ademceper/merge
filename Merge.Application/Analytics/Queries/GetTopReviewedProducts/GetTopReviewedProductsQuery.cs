using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Analytics.Queries.GetTopReviewedProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTopReviewedProductsQuery(
    int Limit
) : IRequest<List<TopReviewedProductDto>>;


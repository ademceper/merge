using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Review.Queries.GetReviewById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetReviewByIdQuery(
    Guid ReviewId
) : IRequest<ReviewDto?>;

using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetMostHelpfulReviews;

public record GetMostHelpfulReviewsQuery(
    Guid ProductId,
    int Limit = 10
) : IRequest<IEnumerable<ReviewHelpfulnessStatsDto>>;

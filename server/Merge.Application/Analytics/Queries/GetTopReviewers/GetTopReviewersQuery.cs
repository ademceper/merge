using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetTopReviewers;

public record GetTopReviewersQuery(
    int Limit
) : IRequest<List<ReviewerStatsDto>>;


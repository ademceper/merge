using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetTopReviewers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTopReviewersQuery(
    int Limit
) : IRequest<List<ReviewerStatsDto>>;


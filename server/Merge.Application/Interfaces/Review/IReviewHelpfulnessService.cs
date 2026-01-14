using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Review;

public interface IReviewHelpfulnessService
{
    Task MarkReviewHelpfulnessAsync(Guid userId, MarkReviewHelpfulnessDto dto, CancellationToken cancellationToken = default);
    Task RemoveHelpfulnessVoteAsync(Guid userId, Guid reviewId, CancellationToken cancellationToken = default);
    Task<ReviewHelpfulnessStatsDto> GetReviewHelpfulnessStatsAsync(Guid reviewId, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReviewHelpfulnessStatsDto>> GetMostHelpfulReviewsAsync(Guid productId, int limit = 10, CancellationToken cancellationToken = default);
}

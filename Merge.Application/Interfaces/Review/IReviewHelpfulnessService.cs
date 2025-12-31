using Merge.Application.DTOs.Review;
namespace Merge.Application.Interfaces.Review;

public interface IReviewHelpfulnessService
{
    Task MarkReviewHelpfulnessAsync(Guid userId, MarkReviewHelpfulnessDto dto);
    Task RemoveHelpfulnessVoteAsync(Guid userId, Guid reviewId);
    Task<ReviewHelpfulnessStatsDto> GetReviewHelpfulnessStatsAsync(Guid reviewId, Guid? userId = null);
    Task<IEnumerable<ReviewHelpfulnessStatsDto>> GetMostHelpfulReviewsAsync(Guid productId, int limit = 10);
}

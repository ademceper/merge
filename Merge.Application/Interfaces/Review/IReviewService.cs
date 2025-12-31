using Merge.Application.DTOs.Review;

namespace Merge.Application.Interfaces.Review;

public interface IReviewService
{
    Task<ReviewDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReviewDto>> GetByProductIdAsync(Guid productId, int page = 1, int pageSize = 20);
    Task<IEnumerable<ReviewDto>> GetByUserIdAsync(Guid userId);
    Task<ReviewDto> CreateAsync(CreateReviewDto dto);
    Task<ReviewDto> UpdateAsync(Guid id, UpdateReviewDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ApproveReviewAsync(Guid id);
    Task<bool> RejectReviewAsync(Guid id, string reason);
}


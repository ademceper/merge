using Merge.Application.Common;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Interfaces.Review;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public interface IReviewService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<ReviewDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<ReviewDto>> GetByProductIdAsync(Guid productId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<ReviewDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ReviewDto> CreateAsync(CreateReviewDto dto, CancellationToken cancellationToken = default);
    Task<ReviewDto> UpdateAsync(Guid id, UpdateReviewDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ApproveReviewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> RejectReviewAsync(Guid id, string reason, CancellationToken cancellationToken = default);
}


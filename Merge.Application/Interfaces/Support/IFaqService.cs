using Merge.Application.Common;
using Merge.Application.DTOs.Support;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Support;

public interface IFaqService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<FaqDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FaqDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<FaqDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<FaqDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<PagedResult<FaqDto>> GetByCategoryAsync(string category, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<FaqDto>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<FaqDto>> GetPublishedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<FaqDto> CreateAsync(CreateFaqDto dto, CancellationToken cancellationToken = default);
    Task<FaqDto> UpdateAsync(Guid id, UpdateFaqDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default);
}


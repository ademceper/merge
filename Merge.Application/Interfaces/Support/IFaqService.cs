using Merge.Application.Common;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Interfaces.Support;

public interface IFaqService
{
    Task<FaqDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<FaqDto>> GetAllAsync();
    Task<PagedResult<FaqDto>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<FaqDto>> GetByCategoryAsync(string category);
    Task<PagedResult<FaqDto>> GetByCategoryAsync(string category, int page, int pageSize);
    Task<IEnumerable<FaqDto>> GetPublishedAsync();
    Task<PagedResult<FaqDto>> GetPublishedAsync(int page, int pageSize);
    Task<FaqDto> CreateAsync(CreateFaqDto dto);
    Task<FaqDto> UpdateAsync(Guid id, UpdateFaqDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task IncrementViewCountAsync(Guid id);
}


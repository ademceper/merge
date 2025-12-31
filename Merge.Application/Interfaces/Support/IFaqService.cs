using Merge.Application.DTOs.Support;

namespace Merge.Application.Interfaces.Support;

public interface IFaqService
{
    Task<FaqDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<FaqDto>> GetAllAsync();
    Task<IEnumerable<FaqDto>> GetByCategoryAsync(string category);
    Task<IEnumerable<FaqDto>> GetPublishedAsync();
    Task<FaqDto> CreateAsync(CreateFaqDto dto);
    Task<FaqDto> UpdateAsync(Guid id, UpdateFaqDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task IncrementViewCountAsync(Guid id);
}


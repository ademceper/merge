using Merge.Application.Common;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Interfaces.Catalog;

public interface ICategoryService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<CategoryDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<CategoryDto>> GetMainCategoriesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CategoryDto categoryDto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(Guid id, CategoryDto categoryDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}


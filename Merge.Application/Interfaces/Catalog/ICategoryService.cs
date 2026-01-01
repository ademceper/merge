using Merge.Application.Common;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Interfaces.Catalog;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<PagedResult<CategoryDto>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync();
    Task<PagedResult<CategoryDto>> GetMainCategoriesAsync(int page, int pageSize);
    Task<CategoryDto> CreateAsync(CategoryDto categoryDto);
    Task<CategoryDto> UpdateAsync(Guid id, CategoryDto categoryDto);
    Task<bool> DeleteAsync(Guid id);
}


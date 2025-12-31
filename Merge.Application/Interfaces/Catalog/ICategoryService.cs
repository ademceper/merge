using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Interfaces.Catalog;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync();
    Task<CategoryDto> CreateAsync(CategoryDto categoryDto);
    Task<CategoryDto> UpdateAsync(Guid id, CategoryDto categoryDto);
    Task<bool> DeleteAsync(Guid id);
}


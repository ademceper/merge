using Merge.Application.DTOs.Product;

namespace Merge.Application.Interfaces.Product;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20);
    Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 20);
    Task<ProductDto> CreateAsync(ProductDto productDto);
    Task<ProductDto> UpdateAsync(Guid id, ProductDto productDto);
    Task<bool> DeleteAsync(Guid id);
}


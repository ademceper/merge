using Merge.Application.DTOs.Product;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
namespace Merge.Application.Interfaces.Product;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(ProductDto productDto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, ProductDto productDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}


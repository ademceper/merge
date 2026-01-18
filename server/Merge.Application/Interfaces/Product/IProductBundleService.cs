using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Interfaces.Product;

public interface IProductBundleService
{
    Task<ProductBundleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductBundleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductBundleDto>> GetActiveBundlesAsync(CancellationToken cancellationToken = default);
    Task<ProductBundleDto> CreateAsync(CreateProductBundleDto dto, CancellationToken cancellationToken = default);
    Task<ProductBundleDto> UpdateAsync(Guid id, UpdateProductBundleDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AddProductToBundleAsync(Guid bundleId, AddProductToBundleDto dto, CancellationToken cancellationToken = default);
    Task<bool> RemoveProductFromBundleAsync(Guid bundleId, Guid productId, CancellationToken cancellationToken = default);
}


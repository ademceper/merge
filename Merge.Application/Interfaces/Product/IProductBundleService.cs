using Merge.Application.DTOs.Product;

namespace Merge.Application.Interfaces.Product;

public interface IProductBundleService
{
    Task<ProductBundleDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductBundleDto>> GetAllAsync();
    Task<IEnumerable<ProductBundleDto>> GetActiveBundlesAsync();
    Task<ProductBundleDto> CreateAsync(CreateProductBundleDto dto);
    Task<ProductBundleDto> UpdateAsync(Guid id, UpdateProductBundleDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> AddProductToBundleAsync(Guid bundleId, AddProductToBundleDto dto);
    Task<bool> RemoveProductFromBundleAsync(Guid bundleId, Guid productId);
}


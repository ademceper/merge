using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Interfaces.Product;

public interface ISizeGuideService
{
    Task<SizeGuideDto> CreateSizeGuideAsync(CreateSizeGuideDto dto, CancellationToken cancellationToken = default);
    Task<SizeGuideDto?> GetSizeGuideAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SizeGuideDto>> GetSizeGuidesByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SizeGuideDto>> GetAllSizeGuidesAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateSizeGuideAsync(Guid id, CreateSizeGuideDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSizeGuideAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductSizeGuideDto?> GetProductSizeGuideAsync(Guid productId, CancellationToken cancellationToken = default);
    Task AssignSizeGuideToProductAsync(AssignSizeGuideDto dto, CancellationToken cancellationToken = default);
    Task<bool> RemoveSizeGuideFromProductAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<SizeRecommendationDto> GetSizeRecommendationAsync(Guid productId, decimal height, decimal weight, decimal? chest = null, decimal? waist = null, CancellationToken cancellationToken = default);
}

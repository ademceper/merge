using Merge.Application.DTOs.Product;

namespace Merge.Application.Interfaces.Product;

public interface ISizeGuideService
{
    Task<SizeGuideDto> CreateSizeGuideAsync(CreateSizeGuideDto dto);
    Task<SizeGuideDto?> GetSizeGuideAsync(Guid id);
    Task<IEnumerable<SizeGuideDto>> GetSizeGuidesByCategoryAsync(Guid categoryId);
    Task<IEnumerable<SizeGuideDto>> GetAllSizeGuidesAsync();
    Task<bool> UpdateSizeGuideAsync(Guid id, CreateSizeGuideDto dto);
    Task<bool> DeleteSizeGuideAsync(Guid id);

    Task<ProductSizeGuideDto?> GetProductSizeGuideAsync(Guid productId);
    Task AssignSizeGuideToProductAsync(AssignSizeGuideDto dto);
    Task<bool> RemoveSizeGuideFromProductAsync(Guid productId);

    Task<SizeRecommendationDto> GetSizeRecommendationAsync(Guid productId, decimal height, decimal weight, decimal? chest = null, decimal? waist = null);
}

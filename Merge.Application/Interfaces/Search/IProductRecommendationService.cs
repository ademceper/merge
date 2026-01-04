using Merge.Application.DTOs.Product;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Search;

public interface IProductRecommendationService
{
    Task<IEnumerable<ProductRecommendationDto>> GetSimilarProductsAsync(Guid productId, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductRecommendationDto>> GetFrequentlyBoughtTogetherAsync(Guid productId, int maxResults = 5, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductRecommendationDto>> GetPersonalizedRecommendationsAsync(Guid userId, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductRecommendationDto>> GetBasedOnViewHistoryAsync(Guid userId, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductRecommendationDto>> GetTrendingProductsAsync(int days = 7, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductRecommendationDto>> GetBestSellersAsync(int maxResults = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductRecommendationDto>> GetNewArrivalsAsync(int days = 30, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<PersonalizedRecommendationsDto> GetCompleteRecommendationsAsync(Guid userId, CancellationToken cancellationToken = default);
}

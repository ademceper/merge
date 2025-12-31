using Merge.Application.DTOs.Product;

namespace Merge.Application.Interfaces.Search;

public interface IProductRecommendationService
{
    Task<IEnumerable<ProductRecommendationDto>> GetSimilarProductsAsync(Guid productId, int maxResults = 10);
    Task<IEnumerable<ProductRecommendationDto>> GetFrequentlyBoughtTogetherAsync(Guid productId, int maxResults = 5);
    Task<IEnumerable<ProductRecommendationDto>> GetPersonalizedRecommendationsAsync(Guid userId, int maxResults = 10);
    Task<IEnumerable<ProductRecommendationDto>> GetBasedOnViewHistoryAsync(Guid userId, int maxResults = 10);
    Task<IEnumerable<ProductRecommendationDto>> GetTrendingProductsAsync(int days = 7, int maxResults = 10);
    Task<IEnumerable<ProductRecommendationDto>> GetBestSellersAsync(int maxResults = 10);
    Task<IEnumerable<ProductRecommendationDto>> GetNewArrivalsAsync(int days = 30, int maxResults = 10);
    Task<PersonalizedRecommendationsDto> GetCompleteRecommendationsAsync(Guid userId);
}

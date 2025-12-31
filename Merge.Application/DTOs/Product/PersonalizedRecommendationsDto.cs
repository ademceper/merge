namespace Merge.Application.DTOs.Product;

public class PersonalizedRecommendationsDto
{
    public List<ProductRecommendationDto> ForYou { get; set; } = new();
    public List<ProductRecommendationDto> BasedOnHistory { get; set; } = new();
    public List<ProductRecommendationDto> Trending { get; set; } = new();
    public List<ProductRecommendationDto> BestSellers { get; set; } = new();
}

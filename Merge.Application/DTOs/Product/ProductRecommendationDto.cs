namespace Merge.Application.DTOs.Product;

public class ProductRecommendationDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string RecommendationReason { get; set; } = string.Empty;
    public decimal RecommendationScore { get; set; }
}

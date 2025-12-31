namespace Merge.Application.DTOs.Product;

public class SizeRecommendationDto
{
    public string RecommendedSize { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty; // High, Medium, Low
    public List<string> AlternativeSizes { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
}

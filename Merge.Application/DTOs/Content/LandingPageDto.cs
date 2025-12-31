namespace Merge.Application.DTOs.Content;

public class LandingPageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Template { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public int ViewCount { get; set; }
    public int ConversionCount { get; set; }
    public decimal ConversionRate { get; set; }
    public bool EnableABTesting { get; set; }
    public Guid? VariantOfId { get; set; }
    public int TrafficSplit { get; set; }
    public DateTime CreatedAt { get; set; }
}

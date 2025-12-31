namespace Merge.Application.DTOs.Content;

public class SEOSettingsDto
{
    public Guid Id { get; set; }
    public string PageType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? CanonicalUrl { get; set; }
    public string? OgTitle { get; set; }
    public string? OgDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public string? TwitterCard { get; set; }
    public Dictionary<string, object>? StructuredData { get; set; }
    public bool IsIndexed { get; set; }
    public bool FollowLinks { get; set; }
    public decimal Priority { get; set; }
    public string? ChangeFrequency { get; set; }
    public DateTime CreatedAt { get; set; }
}

namespace Merge.Application.DTOs.LiveCommerce;

public class LiveStreamProductDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal? ProductPrice { get; set; }
    public decimal? SpecialPrice { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsHighlighted { get; set; }
    public DateTime? ShowcasedAt { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    public int OrderCount { get; set; }
    public string? ShowcaseNotes { get; set; }
}

namespace Merge.Application.DTOs.Content;

public class SitemapEntryDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string PageType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public DateTime LastModified { get; set; }
    public string ChangeFrequency { get; set; } = string.Empty;
    public decimal Priority { get; set; }
    public bool IsActive { get; set; }
}

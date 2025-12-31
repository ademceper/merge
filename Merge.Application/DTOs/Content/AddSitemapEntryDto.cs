namespace Merge.Application.DTOs.Content;

public class AddSitemapEntryDto
{
    public string Url { get; set; } = string.Empty;
    public string PageType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? ChangeFrequency { get; set; }
    public decimal? Priority { get; set; }
}

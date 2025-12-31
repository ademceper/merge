namespace Merge.Application.DTOs.Support;

public class FaqDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int ViewCount { get; set; }
    public bool IsPublished { get; set; }
}

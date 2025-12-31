namespace Merge.Application.DTOs.Marketing;

public class EmailTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Thumbnail { get; set; }
    public List<string> Variables { get; set; } = new();
}

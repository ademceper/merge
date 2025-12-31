namespace Merge.Application.DTOs.Content;

public class CMSPageDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string PageType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Template { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsHomePage { get; set; }
    public int DisplayOrder { get; set; }
    public bool ShowInMenu { get; set; }
    public string? MenuTitle { get; set; }
    public Guid? ParentPageId { get; set; }
    public string? ParentPageTitle { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

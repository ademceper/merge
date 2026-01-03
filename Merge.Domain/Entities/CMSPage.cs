using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// CMSPage Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CMSPage : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // HTML/Markdown content
    public string? Excerpt { get; set; }
    public string PageType { get; set; } = "Page"; // Page, Landing, Custom
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Template { get; set; } // Template name for rendering
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsHomePage { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public bool ShowInMenu { get; set; } = true;
    public string? MenuTitle { get; set; } // Different title for menu display
    public Guid? ParentPageId { get; set; } // For hierarchical pages
    public CMSPage? ParentPage { get; set; }
    public ICollection<CMSPage> ChildPages { get; set; } = new List<CMSPage>();
    public int ViewCount { get; set; } = 0;
}


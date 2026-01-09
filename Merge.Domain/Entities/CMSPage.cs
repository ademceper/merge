using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// CMSPage Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class CMSPage : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty; // HTML/Markdown content
    public string? Excerpt { get; private set; }
    public string PageType { get; private set; } = "Page"; // Page, Landing, Custom
    public ContentStatus Status { get; private set; } = ContentStatus.Draft;
    public Guid? AuthorId { get; private set; }
    public User? Author { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public string? Template { get; private set; } // Template name for rendering
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }
    public bool IsHomePage { get; private set; } = false;
    public int DisplayOrder { get; private set; } = 0;
    public bool ShowInMenu { get; private set; } = true;
    public string? MenuTitle { get; private set; } // Different title for menu display
    public Guid? ParentPageId { get; private set; } // For hierarchical pages
    public int ViewCount { get; private set; } = 0;

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public CMSPage? ParentPage { get; private set; }
    public ICollection<CMSPage> ChildPages { get; private set; } = new List<CMSPage>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private CMSPage() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static CMSPage Create(
        string title,
        string content,
        Guid? authorId = null,
        string? excerpt = null,
        string pageType = "Page",
        ContentStatus status = ContentStatus.Draft,
        string? template = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null,
        bool isHomePage = false,
        int displayOrder = 0,
        bool showInMenu = true,
        string? menuTitle = null,
        Guid? parentPageId = null)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));

        if (parentPageId.HasValue && parentPageId.Value == Guid.Empty)
        {
            throw new DomainException("Geçersiz parent page ID.");
        }

        var slug = GenerateSlug(title);

        var page = new CMSPage
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = slug,
            Content = content,
            Excerpt = excerpt,
            PageType = pageType,
            Status = status,
            AuthorId = authorId,
            Template = template,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            IsHomePage = isHomePage,
            DisplayOrder = displayOrder,
            ShowInMenu = showInMenu,
            MenuTitle = menuTitle,
            ParentPageId = parentPageId,
            PublishedAt = status == ContentStatus.Published ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - CMSPageCreatedEvent yayınla (ÖNERİLİR)
        page.AddDomainEvent(new CMSPageCreatedEvent(page.Id, title, slug, authorId));

        return page;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update title
    public void UpdateTitle(string newTitle)
    {
        Guard.AgainstNullOrEmpty(newTitle, nameof(newTitle));
        Title = newTitle;
        Slug = GenerateSlug(newTitle);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, newTitle, Slug));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update content
    public void UpdateContent(string newContent)
    {
        Guard.AgainstNullOrEmpty(newContent, nameof(newContent));
        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update excerpt
    public void UpdateExcerpt(string? newExcerpt)
    {
        Excerpt = newExcerpt;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update page type
    public void UpdatePageType(string newPageType)
    {
        Guard.AgainstNullOrEmpty(newPageType, nameof(newPageType));
        PageType = newPageType;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update status
    public void UpdateStatus(ContentStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == ContentStatus.Published && !PublishedAt.HasValue)
        {
            PublishedAt = DateTime.UtcNow;
            // ✅ BOLUM 1.5: Domain Events - CMSPagePublishedEvent yayınla (ÖNERİLİR)
            AddDomainEvent(new CMSPagePublishedEvent(Id, Title, Slug));
        }
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Publish page
    public void Publish()
    {
        if (Status == ContentStatus.Published)
            return;

        Status = ContentStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPagePublishedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPagePublishedEvent(Id, Title, Slug));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update template
    public void UpdateTemplate(string? newTemplate)
    {
        Template = newTemplate;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update meta information
    public void UpdateMetaInformation(string? metaTitle, string? metaDescription, string? metaKeywords)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set as home page
    public void SetAsHomePage()
    {
        if (IsHomePage)
            return;

        IsHomePage = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Unset as home page
    public void UnsetAsHomePage()
    {
        if (!IsHomePage)
            return;

        IsHomePage = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update display order
    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstNegative(newDisplayOrder, nameof(newDisplayOrder));
        DisplayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update show in menu
    public void UpdateShowInMenu(bool showInMenu)
    {
        ShowInMenu = showInMenu;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update menu title
    public void UpdateMenuTitle(string? newMenuTitle)
    {
        MenuTitle = newMenuTitle;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update parent page
    public void UpdateParentPage(Guid? parentPageId)
    {
        if (parentPageId.HasValue && parentPageId.Value == Id)
        {
            throw new DomainException("Sayfa kendisinin alt sayfası olamaz.");
        }
        ParentPageId = parentPageId;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Increment view count
    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageDeletedEvent(Id, Title));
    }

    // ✅ BOLUM 1.3: Slug generation helper
    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "");

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }
}



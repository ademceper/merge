using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// CMSPage Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class CMSPage : BaseEntity, IAggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
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

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public CMSPage? ParentPage { get; private set; }
    
    private readonly List<CMSPage> _childPages = new();
    public IReadOnlyCollection<CMSPage> ChildPages => _childPages.AsReadOnly();

    private CMSPage() { }

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
        // Configuration değerleri: MaxCMSPageTitleLength=200, MaxCMSPageContentLength=50000, MaxCMSPageExcerptLength=500, MaxPageTypeLength=50
        Guard.AgainstLength(title, 200, nameof(title));
        Guard.AgainstLength(content, 50000, nameof(content));
        Guard.AgainstLength(pageType, 50, nameof(pageType));
        if (excerpt != null)
            Guard.AgainstLength(excerpt, 500, nameof(excerpt));
        // Configuration değerleri: MaxTemplateNameLength=100, MaxMenuTitleLength=200
        if (template != null)
            Guard.AgainstLength(template, 100, nameof(template));
        if (menuTitle != null)
            Guard.AgainstLength(menuTitle, 200, nameof(menuTitle));
        // Configuration değerleri: MaxMetaTitleLength=60, MaxMetaDescriptionLength=160, MaxMetaKeywordsLength=255
        if (metaTitle != null)
            Guard.AgainstLength(metaTitle, 60, nameof(metaTitle));
        if (metaDescription != null)
            Guard.AgainstLength(metaDescription, 160, nameof(metaDescription));
        if (metaKeywords != null)
            Guard.AgainstLength(metaKeywords, 255, nameof(metaKeywords));

        if (parentPageId.HasValue && parentPageId.Value == Guid.Empty)
        {
            throw new DomainException("Geçersiz parent page ID.");
        }

        var slug = Slug.FromString(title);

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

        page.AddDomainEvent(new CMSPageCreatedEvent(page.Id, title, slug.Value, authorId));

        return page;
    }

    public void UpdateTitle(string newTitle)
    {
        Guard.AgainstNullOrEmpty(newTitle, nameof(newTitle));
        // Configuration değeri: MaxCMSPageTitleLength=200
        Guard.AgainstLength(newTitle, 200, nameof(newTitle));
        Title = newTitle;
        Slug = Slug.FromString(newTitle);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, newTitle, Slug.Value));
    }

    public void UpdateSlug(string newSlug)
    {
        Guard.AgainstNullOrEmpty(newSlug, nameof(newSlug));
        Slug = Slug.FromString(newSlug);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateContent(string newContent)
    {
        Guard.AgainstNullOrEmpty(newContent, nameof(newContent));
        // Configuration değeri: MaxCMSPageContentLength=50000
        Guard.AgainstLength(newContent, 50000, nameof(newContent));
        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateExcerpt(string? newExcerpt)
    {
        // Configuration değeri: MaxCMSPageExcerptLength=500
        if (newExcerpt != null)
            Guard.AgainstLength(newExcerpt, 500, nameof(newExcerpt));
        Excerpt = newExcerpt;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdatePageType(string newPageType)
    {
        Guard.AgainstNullOrEmpty(newPageType, nameof(newPageType));
        // Configuration değeri: MaxPageTypeLength=50
        Guard.AgainstLength(newPageType, 50, nameof(newPageType));
        PageType = newPageType;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateStatus(ContentStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == ContentStatus.Published && !PublishedAt.HasValue)
        {
            PublishedAt = DateTime.UtcNow;
            AddDomainEvent(new CMSPagePublishedEvent(Id, Title, Slug.Value));
        }
        else
        {
            AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status == ContentStatus.Published)
            return;

        Status = ContentStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPagePublishedEvent(Id, Title, Slug.Value));
    }

    public void Unpublish()
    {
        if (Status == ContentStatus.Draft)
            return;

        Status = ContentStatus.Draft;
        PublishedAt = null;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUnpublishedEvent(Id, Title, Slug.Value));
    }

    public void UpdateTemplate(string? newTemplate)
    {
        // Configuration değeri: MaxTemplateNameLength=100
        if (newTemplate != null)
            Guard.AgainstLength(newTemplate, 100, nameof(newTemplate));
        Template = newTemplate;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateMetaInformation(string? metaTitle, string? metaDescription, string? metaKeywords)
    {
        // Configuration değerleri: MaxMetaTitleLength=60, MaxMetaDescriptionLength=160, MaxMetaKeywordsLength=255
        if (metaTitle != null)
            Guard.AgainstLength(metaTitle, 60, nameof(metaTitle));
        if (metaDescription != null)
            Guard.AgainstLength(metaDescription, 160, nameof(metaDescription));
        if (metaKeywords != null)
            Guard.AgainstLength(metaKeywords, 255, nameof(metaKeywords));
        
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void SetAsHomePage()
    {
        if (IsHomePage)
            return;

        IsHomePage = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UnsetAsHomePage()
    {
        if (!IsHomePage)
            return;

        IsHomePage = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstNegative(newDisplayOrder, nameof(newDisplayOrder));
        DisplayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateShowInMenu(bool showInMenu)
    {
        ShowInMenu = showInMenu;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateMenuTitle(string? newMenuTitle)
    {
        // Configuration değeri: MaxMenuTitleLength=200
        if (newMenuTitle != null)
            Guard.AgainstLength(newMenuTitle, 200, nameof(newMenuTitle));
        MenuTitle = newMenuTitle;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateParentPage(Guid? parentPageId)
    {
        if (parentPageId.HasValue && parentPageId.Value == Id)
        {
            throw new DomainException("Sayfa kendisinin alt sayfası olamaz.");
        }
        ParentPageId = parentPageId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void UpdateAuthor(Guid? newAuthorId)
    {
        AuthorId = newAuthorId;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageViewedEvent(Id, Title, ViewCount));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageDeletedEvent(Id, Title));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CMSPageRestoredEvent(Id, Title, Slug.Value));
    }

}



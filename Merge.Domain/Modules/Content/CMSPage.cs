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
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Title { get; private set; } = string.Empty;
    // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı (ZORUNLU)
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

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public CMSPage? ParentPage { get; private set; }
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    private readonly List<CMSPage> _childPages = new();
    public IReadOnlyCollection<CMSPage> ChildPages => _childPages.AsReadOnly();

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
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxCMSPageTitleLength=200, MaxCMSPageContentLength=50000, MaxCMSPageExcerptLength=500, MaxPageTypeLength=50
        Guard.AgainstLength(title, 200, nameof(title));
        Guard.AgainstLength(content, 50000, nameof(content));
        Guard.AgainstLength(pageType, 50, nameof(pageType));
        if (excerpt != null)
            Guard.AgainstLength(excerpt, 500, nameof(excerpt));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxTemplateNameLength=100, MaxMenuTitleLength=200
        if (template != null)
            Guard.AgainstLength(template, 100, nameof(template));
        if (menuTitle != null)
            Guard.AgainstLength(menuTitle, 200, nameof(menuTitle));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
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

        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
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

        // ✅ BOLUM 1.5: Domain Events - CMSPageCreatedEvent yayınla (ÖNERİLİR)
        page.AddDomainEvent(new CMSPageCreatedEvent(page.Id, title, slug.Value, authorId));

        return page;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update title
    public void UpdateTitle(string newTitle)
    {
        Guard.AgainstNullOrEmpty(newTitle, nameof(newTitle));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxCMSPageTitleLength=200
        Guard.AgainstLength(newTitle, 200, nameof(newTitle));
        Title = newTitle;
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        Slug = Slug.FromString(newTitle);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, newTitle, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update slug (manual slug update)
    public void UpdateSlug(string newSlug)
    {
        Guard.AgainstNullOrEmpty(newSlug, nameof(newSlug));
        // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
        Slug = Slug.FromString(newSlug);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update content
    public void UpdateContent(string newContent)
    {
        Guard.AgainstNullOrEmpty(newContent, nameof(newContent));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxCMSPageContentLength=50000
        Guard.AgainstLength(newContent, 50000, nameof(newContent));
        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update excerpt
    public void UpdateExcerpt(string? newExcerpt)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxCMSPageExcerptLength=500
        if (newExcerpt != null)
            Guard.AgainstLength(newExcerpt, 500, nameof(newExcerpt));
        Excerpt = newExcerpt;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update page type
    public void UpdatePageType(string newPageType)
    {
        Guard.AgainstNullOrEmpty(newPageType, nameof(newPageType));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPageTypeLength=50
        Guard.AgainstLength(newPageType, 50, nameof(newPageType));
        PageType = newPageType;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update status
    public void UpdateStatus(ContentStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == ContentStatus.Published && !PublishedAt.HasValue)
        {
            PublishedAt = DateTime.UtcNow;
            // ✅ BOLUM 1.5: Domain Events - CMSPagePublishedEvent yayınla (ÖNERİLİR)
            AddDomainEvent(new CMSPagePublishedEvent(Id, Title, Slug.Value));
        }
        else
        {
            // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
            AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
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
        AddDomainEvent(new CMSPagePublishedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Unpublish page
    public void Unpublish()
    {
        if (Status == ContentStatus.Draft)
            return;

        Status = ContentStatus.Draft;
        PublishedAt = null;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUnpublishedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUnpublishedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update template
    public void UpdateTemplate(string? newTemplate)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxTemplateNameLength=100
        if (newTemplate != null)
            Guard.AgainstLength(newTemplate, 100, nameof(newTemplate));
        Template = newTemplate;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update meta information
    public void UpdateMetaInformation(string? metaTitle, string? metaDescription, string? metaKeywords)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
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
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set as home page
    public void SetAsHomePage()
    {
        if (IsHomePage)
            return;

        IsHomePage = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Unset as home page
    public void UnsetAsHomePage()
    {
        if (!IsHomePage)
            return;

        IsHomePage = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update display order
    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstNegative(newDisplayOrder, nameof(newDisplayOrder));
        DisplayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update show in menu
    public void UpdateShowInMenu(bool showInMenu)
    {
        ShowInMenu = showInMenu;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update menu title
    public void UpdateMenuTitle(string? newMenuTitle)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxMenuTitleLength=200
        if (newMenuTitle != null)
            Guard.AgainstLength(newMenuTitle, 200, nameof(newMenuTitle));
        MenuTitle = newMenuTitle;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
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
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update author
    public void UpdateAuthor(Guid? newAuthorId)
    {
        AuthorId = newAuthorId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageUpdatedEvent(Id, Title, Slug.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Increment view count
    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageViewedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageViewedEvent(Id, Title, ViewCount));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageDeletedEvent(Id, Title));
    }

    // ✅ BOLUM 1.1: Domain Logic - Restore deleted page
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - CMSPageRestoredEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new CMSPageRestoredEvent(Id, Title, Slug.Value));
    }

}



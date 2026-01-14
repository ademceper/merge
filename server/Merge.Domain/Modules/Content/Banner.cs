using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// Banner Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class Banner : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public string? LinkUrl { get; private set; }
    public string Position { get; private set; } = "Homepage"; // Homepage, Category, Product, Cart, etc.
    public int SortOrder { get; private set; } = 0;
    public bool IsActive { get; private set; } = true;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? ProductId { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Category? Category { get; private set; }
    public Product? Product { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Banner() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Banner Create(
        string title,
        string imageUrl,
        string position = "Homepage",
        string? description = null,
        string? linkUrl = null,
        int sortOrder = 0,
        bool isActive = true,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? categoryId = null,
        Guid? productId = null)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(imageUrl, nameof(imageUrl));
        Guard.AgainstNullOrEmpty(position, nameof(position));
        Guard.AgainstNegative(sortOrder, nameof(sortOrder));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxBannerTitleLength=200, MaxBannerDescriptionLength=1000, MaxBannerPositionLength=50
        Guard.AgainstLength(title, 200, nameof(title));
        Guard.AgainstLength(position, 50, nameof(position));
        if (description != null)
            Guard.AgainstLength(description, 1000, nameof(description));

        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!string.IsNullOrEmpty(imageUrl) && !IsValidUrl(imageUrl))
        {
            throw new DomainException("Geçerli bir image URL giriniz.");
        }

        if (!string.IsNullOrEmpty(linkUrl) && !IsValidUrl(linkUrl))
        {
            throw new DomainException("Geçerli bir link URL giriniz.");
        }

        if (endDate.HasValue && startDate.HasValue && endDate.Value <= startDate.Value)
        {
            throw new DomainException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        var banner = new Banner
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            LinkUrl = linkUrl,
            Position = position,
            SortOrder = sortOrder,
            IsActive = isActive,
            StartDate = startDate,
            EndDate = endDate,
            CategoryId = categoryId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - BannerCreatedEvent yayınla (ÖNERİLİR)
        banner.AddDomainEvent(new BannerCreatedEvent(banner.Id, title, position));

        return banner;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update title
    public void UpdateTitle(string newTitle)
    {
        Guard.AgainstNullOrEmpty(newTitle, nameof(newTitle));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxBannerTitleLength=200
        Guard.AgainstLength(newTitle, 200, nameof(newTitle));
        Title = newTitle;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, newTitle, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update description
    public void UpdateDescription(string? newDescription)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxBannerDescriptionLength=1000
        if (newDescription != null)
            Guard.AgainstLength(newDescription, 1000, nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update image URL
    public void UpdateImageUrl(string newImageUrl)
    {
        Guard.AgainstNullOrEmpty(newImageUrl, nameof(newImageUrl));
        
        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!IsValidUrl(newImageUrl))
        {
            throw new DomainException("Geçerli bir image URL giriniz.");
        }
        
        ImageUrl = newImageUrl;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update link URL
    public void UpdateLinkUrl(string? newLinkUrl)
    {
        // ✅ BOLUM 1.3: URL Validation - Domain layer'da URL validasyonu
        if (!string.IsNullOrEmpty(newLinkUrl) && !IsValidUrl(newLinkUrl))
        {
            throw new DomainException("Geçerli bir link URL giriniz.");
        }
        
        LinkUrl = newLinkUrl;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update position
    public void UpdatePosition(string newPosition)
    {
        Guard.AgainstNullOrEmpty(newPosition, nameof(newPosition));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxBannerPositionLength=50
        Guard.AgainstLength(newPosition, 50, nameof(newPosition));
        Position = newPosition;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, Title, newPosition));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update sort order
    public void UpdateSortOrder(int newSortOrder)
    {
        Guard.AgainstNegative(newSortOrder, nameof(newSortOrder));
        SortOrder = newSortOrder;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate banner
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerActivatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerActivatedEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate banner
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerDeactivatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerDeactivatedEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update date range
    public void UpdateDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (endDate.HasValue && startDate.HasValue && endDate.Value <= startDate.Value)
        {
            throw new DomainException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update category
    public void UpdateCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update product
    public void UpdateProduct(Guid? productId)
    {
        ProductId = productId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if banner is available
    public bool IsAvailable()
    {
        if (!IsActive)
            return false;

        var now = DateTime.UtcNow;
        if (StartDate.HasValue && now < StartDate.Value)
            return false;

        if (EndDate.HasValue && now > EndDate.Value)
            return false;

        return true;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerDeletedEvent(Id, Title));
    }

    // ✅ BOLUM 1.1: Domain Logic - Restore deleted banner
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerRestoredEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerRestoredEvent(Id, Title, Position));
    }

    // ✅ BOLUM 1.3: URL Validation Helper Method
    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}



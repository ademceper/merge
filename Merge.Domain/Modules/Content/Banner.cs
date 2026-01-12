using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
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
        Title = newTitle;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerUpdatedEvent(Id, newTitle, Position));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update description
    public void UpdateDescription(string? newDescription)
    {
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update image URL
    public void UpdateImageUrl(string newImageUrl)
    {
        Guard.AgainstNullOrEmpty(newImageUrl, nameof(newImageUrl));
        ImageUrl = newImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update link URL
    public void UpdateLinkUrl(string? newLinkUrl)
    {
        LinkUrl = newLinkUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update position
    public void UpdatePosition(string newPosition)
    {
        Guard.AgainstNullOrEmpty(newPosition, nameof(newPosition));
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
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate banner
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate banner
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
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
    }

    // ✅ BOLUM 1.1: Domain Logic - Update category
    public void UpdateCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update product
    public void UpdateProduct(Guid? productId)
    {
        ProductId = productId;
        UpdatedAt = DateTime.UtcNow;
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
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - BannerDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new BannerDeletedEvent(Id, Title));
    }
}



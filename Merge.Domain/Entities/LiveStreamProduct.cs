using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// LiveStreamProduct Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveStreamProduct : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid LiveStreamId { get; private set; }
    public LiveStream LiveStream { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    private int _displayOrder = 0;
    public int DisplayOrder 
    { 
        get => _displayOrder; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(DisplayOrder));
            _displayOrder = value;
        } 
    }
    
    public bool IsHighlighted { get; private set; } = false; // Currently showcased
    public DateTime? ShowcasedAt { get; private set; } // When product was showcased
    
    private int _viewCount = 0;
    public int ViewCount 
    { 
        get => _viewCount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(ViewCount));
            _viewCount = value;
        } 
    }
    
    private int _clickCount = 0;
    public int ClickCount 
    { 
        get => _clickCount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(ClickCount));
            _clickCount = value;
        } 
    }
    
    private int _orderCount = 0;
    public int OrderCount 
    { 
        get => _orderCount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(OrderCount));
            _orderCount = value;
        } 
    }
    
    private decimal? _specialPrice;
    public decimal? SpecialPrice 
    { 
        get => _specialPrice; 
        private set 
        { 
            if (value.HasValue)
            {
                Guard.AgainstNegative(value.Value, nameof(SpecialPrice));
            }
            _specialPrice = value;
        } 
    }
    
    public string? ShowcaseNotes { get; private set; } // Notes about the product showcase

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private LiveStreamProduct() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static LiveStreamProduct Create(
        Guid liveStreamId,
        Guid productId,
        int displayOrder = 0,
        decimal? specialPrice = null,
        string? showcaseNotes = null)
    {
        Guard.AgainstDefault(liveStreamId, nameof(liveStreamId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));

        var streamProduct = new LiveStreamProduct
        {
            Id = Guid.NewGuid(),
            LiveStreamId = liveStreamId,
            ProductId = productId,
            _displayOrder = displayOrder,
            _specialPrice = specialPrice,
            ShowcaseNotes = showcaseNotes,
            IsHighlighted = false,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - LiveStreamProductAddedEvent
        streamProduct.AddDomainEvent(new LiveStreamProductAddedEvent(liveStreamId, productId, specialPrice));

        return streamProduct;
    }

    // ✅ BOLUM 1.1: Domain Method - Showcase product
    public void Showcase()
    {
        if (IsHighlighted) return;

        IsHighlighted = true;
        ShowcasedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - LiveStreamProductShowcasedEvent
        AddDomainEvent(new LiveStreamProductShowcasedEvent(LiveStreamId, ProductId, ShowcasedAt.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - Unhighlight product
    public void Unhighlight()
    {
        if (!IsHighlighted) return;

        IsHighlighted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Increment view count
    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Increment click count
    public void IncrementClickCount()
    {
        ClickCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Increment order count
    public void IncrementOrderCount()
    {
        OrderCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update display order
    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstNegative(newDisplayOrder, nameof(newDisplayOrder));

        _displayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update special price
    public void UpdateSpecialPrice(decimal? newSpecialPrice)
    {
        if (newSpecialPrice.HasValue)
        {
            Guard.AgainstNegative(newSpecialPrice.Value, nameof(newSpecialPrice));
        }

        _specialPrice = newSpecialPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsHighlighted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}


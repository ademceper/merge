using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// LiveStreamProduct Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveStreamProduct : BaseEntity, IAggregateRoot
{
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
    
    [NotMapped]
    public Money? SpecialPriceMoney => _specialPrice.HasValue ? new Money(_specialPrice.Value) : null;
    
    private string? _showcaseNotes;
    public string? ShowcaseNotes 
    { 
        get => _showcaseNotes; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstLength(value, 1000, nameof(ShowcaseNotes));
            }
            _showcaseNotes = value;
        } 
    }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private LiveStreamProduct() { }

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
            _showcaseNotes = showcaseNotes,
            IsHighlighted = false,
            CreatedAt = DateTime.UtcNow
        };

        streamProduct.ValidateInvariants();

        streamProduct.AddDomainEvent(new LiveStreamProductAddedEvent(liveStreamId, productId, specialPrice));

        return streamProduct;
    }

    public void Showcase()
    {
        if (IsHighlighted) return;

        IsHighlighted = true;
        ShowcasedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamProductShowcasedEvent(LiveStreamId, ProductId, ShowcasedAt.Value));
    }

    public void Unhighlight()
    {
        if (!IsHighlighted) return;

        IsHighlighted = false;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamProductUnhighlightedEvent(LiveStreamId, ProductId, UpdatedAt.Value));
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void IncrementClickCount()
    {
        ClickCount++;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void IncrementOrderCount()
    {
        OrderCount++;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstNegative(newDisplayOrder, nameof(newDisplayOrder));

        _displayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void UpdateSpecialPrice(decimal? newSpecialPrice)
    {
        if (newSpecialPrice.HasValue)
        {
            Guard.AgainstNegative(newSpecialPrice.Value, nameof(newSpecialPrice));
        }

        _specialPrice = newSpecialPrice;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void UpdateShowcaseNotes(string? newShowcaseNotes)
    {
        ShowcaseNotes = newShowcaseNotes;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsHighlighted = false;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamProductDeletedEvent(LiveStreamId, ProductId, UpdatedAt.Value));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamProductRestoredEvent(LiveStreamId, ProductId, UpdatedAt.Value));
    }

    private void ValidateInvariants()
    {
        if (Guid.Empty == LiveStreamId)
            throw new DomainException("Canlı yayın ID boş olamaz");

        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");

        if (DisplayOrder < 0)
            throw new DomainException("Görüntüleme sırası negatif olamaz");

        if (ViewCount < 0)
            throw new DomainException("Görüntülenme sayısı negatif olamaz");

        if (ClickCount < 0)
            throw new DomainException("Tıklanma sayısı negatif olamaz");

        if (OrderCount < 0)
            throw new DomainException("Sipariş sayısı negatif olamaz");

        if (SpecialPrice.HasValue && SpecialPrice.Value < 0)
            throw new DomainException("Özel fiyat negatif olamaz");
    }
}


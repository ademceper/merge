using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// WholesalePrice Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class WholesalePrice : BaseEntity, IAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid? OrganizationId { get; private set; } // Organization-specific pricing
    public int MinQuantity { get; private set; } // Minimum quantity for this price tier
    public int? MaxQuantity { get; private set; } // Maximum quantity (null = unlimited)
    
    private decimal _price;
    
    // Database column (EF Core mapping)
    public decimal Price 
    { 
        get => _price; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Price));
            _price = value;
        }
    }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money PriceMoney => new Money(_price);
    
    public bool IsActive { get; private set; } = true;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Product Product { get; private set; } = null!;
    public Organization? Organization { get; private set; }

    private WholesalePrice() { }

    public static WholesalePrice Create(
        Guid productId,
        Product product,
        Guid? organizationId,
        Organization? organization,
        int minQuantity,
        int? maxQuantity,
        decimal price,
        bool isActive = true,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNegativeOrZero(minQuantity, nameof(minQuantity));
        Guard.AgainstNegativeOrZero(price, nameof(price));

        if (maxQuantity.HasValue && maxQuantity.Value < minQuantity)
            throw new DomainException("Maksimum miktar minimum miktardan küçük olamaz");

        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");

        var wholesalePrice = new WholesalePrice
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Product = product,
            OrganizationId = organizationId,
            Organization = organization,
            MinQuantity = minQuantity,
            MaxQuantity = maxQuantity,
            Price = price,
            IsActive = isActive,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };

        wholesalePrice.AddDomainEvent(new WholesalePriceCreatedEvent(
            wholesalePrice.Id,
            productId,
            organizationId,
            minQuantity,
            maxQuantity,
            price));

        return wholesalePrice;
    }

    public void UpdatePrice(decimal newPrice)
    {
        Guard.AgainstNegativeOrZero(newPrice, nameof(newPrice));
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WholesalePriceUpdatedEvent(Id, ProductId, OrganizationId, newPrice));
    }

    public void UpdateQuantityRange(int minQuantity, int? maxQuantity)
    {
        Guard.AgainstNegativeOrZero(minQuantity, nameof(minQuantity));

        if (maxQuantity.HasValue && maxQuantity.Value < minQuantity)
            throw new DomainException("Maksimum miktar minimum miktardan küçük olamaz");

        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WholesalePriceUpdatedEvent(Id, ProductId, OrganizationId, Price));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WholesalePriceActivatedEvent(Id, ProductId, OrganizationId));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WholesalePriceDeactivatedEvent(Id, ProductId, OrganizationId));
    }

    public void UpdateDates(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new DomainException("Bitiş tarihi başlangıç tarihinden önce olamaz");

        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WholesalePriceUpdatedEvent(Id, ProductId, OrganizationId, Price));
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Wholesale price zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WholesalePriceDeletedEvent(Id, ProductId, OrganizationId));
    }
}


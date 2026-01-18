using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Catalog;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// FlashSaleProduct Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class FlashSaleProduct : BaseEntity, IAggregateRoot
{
    public Guid FlashSaleId { get; private set; }
    public Guid ProductId { get; private set; }
    
    private decimal _salePrice;
    public decimal SalePrice 
    { 
        get => _salePrice; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(SalePrice));
            _salePrice = value;
        } 
    }
    
    private int _stockLimit;
    public int StockLimit 
    { 
        get => _stockLimit; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(StockLimit));
            _stockLimit = value;
        } 
    }
    
    private int _soldQuantity = 0;
    public int SoldQuantity 
    { 
        get => _soldQuantity; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(SoldQuantity));
            if (_stockLimit > 0 && value > _stockLimit)
                throw new DomainException("Satılan miktar stok limitini aşamaz");
            _soldQuantity = value;
        } 
    }
    
    public int SortOrder { get; private set; } = 0;
    
    // Navigation properties
    public FlashSale FlashSale { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money SalePriceMoney => new Money(_salePrice);

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private FlashSaleProduct() { }

    public static FlashSaleProduct Create(
        Guid flashSaleId,
        Guid productId,
        Money salePrice,
        int stockLimit,
        int sortOrder = 0)
    {
        Guard.AgainstDefault(flashSaleId, nameof(flashSaleId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNull(salePrice, nameof(salePrice));
        Guard.AgainstNegative(stockLimit, nameof(stockLimit));

        var flashSaleProduct = new FlashSaleProduct
        {
            Id = Guid.NewGuid(),
            FlashSaleId = flashSaleId,
            ProductId = productId,
            _salePrice = salePrice.Amount,
            _stockLimit = stockLimit,
            _soldQuantity = 0,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };

        flashSaleProduct.AddDomainEvent(new FlashSaleProductCreatedEvent(
            flashSaleProduct.Id,
            flashSaleId,
            productId,
            salePrice.Amount,
            stockLimit));

        return flashSaleProduct;
    }

    public void RecordSale(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        if (_stockLimit > 0 && _soldQuantity + quantity > _stockLimit)
            throw new DomainException("Stok limiti aşıldı");

        _soldQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FlashSaleProductSaleRecordedEvent(
            Id,
            FlashSaleId,
            ProductId,
            quantity,
            _soldQuantity,
            GetRemainingStock()));
    }

    public bool IsAvailable()
    {
        return _stockLimit == 0 || _soldQuantity < _stockLimit;
    }

    public int GetRemainingStock()
    {
        if (_stockLimit == 0)
            return int.MaxValue; // Unlimited

        return _stockLimit - _soldQuantity;
    }

    public void UpdateSalePrice(Money salePrice)
    {
        Guard.AgainstNull(salePrice, nameof(salePrice));
        _salePrice = salePrice.Amount;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FlashSaleProductUpdatedEvent(Id, FlashSaleId, ProductId, "SalePrice"));
    }

    public void UpdateStockLimit(int stockLimit)
    {
        Guard.AgainstNegative(stockLimit, nameof(stockLimit));

        if (stockLimit > 0 && _soldQuantity > stockLimit)
            throw new DomainException("Yeni stok limiti satılan miktardan az olamaz");

        _stockLimit = stockLimit;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FlashSaleProductUpdatedEvent(Id, FlashSaleId, ProductId, "StockLimit"));
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FlashSaleProductUpdatedEvent(Id, FlashSaleId, ProductId, "SortOrder"));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FlashSaleProductDeletedEvent(Id, FlashSaleId, ProductId));
    }
}


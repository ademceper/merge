using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// FlashSaleProduct Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class FlashSaleProduct : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid FlashSaleId { get; private set; }
    public Guid ProductId { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects - Money backing field (EF Core compatibility)
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
    
    // ✅ BOLUM 1.6: Invariant validation - StockLimit >= 0
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
    
    // ✅ BOLUM 1.6: Invariant validation - SoldQuantity >= 0 && SoldQuantity <= StockLimit
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

    // ✅ BOLUM 1.3: Value Object properties
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money SalePriceMoney => new Money(_salePrice);

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private FlashSaleProduct() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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

        return new FlashSaleProduct
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
    }

    // ✅ BOLUM 1.1: Domain Method - Record sale
    public void RecordSale(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        if (_stockLimit > 0 && _soldQuantity + quantity > _stockLimit)
            throw new DomainException("Stok limiti aşıldı");

        _soldQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Check if available
    public bool IsAvailable()
    {
        return _stockLimit == 0 || _soldQuantity < _stockLimit;
    }

    // ✅ BOLUM 1.1: Domain Method - Get remaining stock
    public int GetRemainingStock()
    {
        if (_stockLimit == 0)
            return int.MaxValue; // Unlimited

        return _stockLimit - _soldQuantity;
    }

    // ✅ BOLUM 1.1: Domain Method - Update sale price
    public void UpdateSalePrice(Money salePrice)
    {
        Guard.AgainstNull(salePrice, nameof(salePrice));
        _salePrice = salePrice.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update stock limit
    public void UpdateStockLimit(int stockLimit)
    {
        Guard.AgainstNegative(stockLimit, nameof(stockLimit));

        if (stockLimit > 0 && _soldQuantity > stockLimit)
            throw new DomainException("Yeni stok limiti satılan miktardan az olamaz");

        _stockLimit = stockLimit;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update sort order
    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}


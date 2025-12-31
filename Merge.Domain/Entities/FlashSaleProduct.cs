namespace Merge.Domain.Entities;

public class FlashSaleProduct : BaseEntity
{
    public Guid FlashSaleId { get; set; }
    public Guid ProductId { get; set; }
    public decimal SalePrice { get; set; }
    public int StockLimit { get; set; } // Flash sale için özel stok limiti
    public int SoldQuantity { get; set; } = 0;
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    public FlashSale FlashSale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}


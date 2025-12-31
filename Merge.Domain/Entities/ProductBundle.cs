namespace Merge.Domain.Entities;

public class ProductBundle : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BundlePrice { get; set; }
    public decimal? OriginalTotalPrice { get; set; } // Tüm ürünlerin toplam fiyatı
    public decimal DiscountPercentage { get; set; } // Paket indirim yüzdesi
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Navigation properties
    public ICollection<BundleItem> BundleItems { get; set; } = new List<BundleItem>();
}


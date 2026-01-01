using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new List<string>();
    public decimal Rating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }
    public Guid? SellerId { get; set; } // Eğer marketplace ise
    public Guid? StoreId { get; set; } // Store assignment for multi-store support

    // ✅ CONCURRENCY: Race condition ve overselling önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Category Category { get; set; } = null!;
    public User? Seller { get; set; }
    public Store? Store { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public ICollection<FlashSaleProduct> FlashSaleProducts { get; set; } = new List<FlashSaleProduct>();
    public ICollection<BundleItem> BundleItems { get; set; } = new List<BundleItem>();
    public ICollection<RecentlyViewedProduct> RecentlyViewedProducts { get; set; } = new List<RecentlyViewedProduct>();
}


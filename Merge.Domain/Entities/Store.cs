using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

public class Store : BaseEntity
{
    public Guid SellerId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // URL-friendly store name
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    public bool IsPrimary { get; set; } = false; // Primary store for seller
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? Settings { get; set; } // JSON for store-specific settings
    
    // Navigation properties
    public User Seller { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}


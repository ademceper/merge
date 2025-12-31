using Microsoft.AspNetCore.Identity;

namespace Merge.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    public ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<RecentlyViewedProduct> RecentlyViewedProducts { get; set; } = new List<RecentlyViewedProduct>();
    public SellerProfile? SellerProfile { get; set; }
    public ICollection<SavedCartItem> SavedCartItems { get; set; } = new List<SavedCartItem>();
    public ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();
    public ICollection<GiftCard> PurchasedGiftCards { get; set; } = new List<GiftCard>();
    public ICollection<GiftCard> AssignedGiftCards { get; set; } = new List<GiftCard>();
    
    // Organization & Team
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
}


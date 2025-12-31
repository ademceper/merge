namespace Merge.Domain.Entities;

public class SellerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? StoreDescription { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Suspended
    public decimal CommissionRate { get; set; } = 0; // Yüzde olarak
    public decimal TotalEarnings { get; set; } = 0;
    public decimal PendingBalance { get; set; } = 0;
    public decimal AvailableBalance { get; set; } = 0;
    public int TotalOrders { get; set; } = 0;
    public int TotalProducts { get; set; } = 0;
    public decimal AverageRating { get; set; } = 0;
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}


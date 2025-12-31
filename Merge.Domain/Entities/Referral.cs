namespace Merge.Domain.Entities;

public class ReferralCode : BaseEntity
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int UsageCount { get; set; } = 0;
    public int MaxUsage { get; set; } = 0; // 0 = unlimited
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int PointsReward { get; set; } = 100;
    public decimal DiscountPercentage { get; set; } = 10;

    // Navigation properties
    public User User { get; set; } = null!;
}

public class Referral : BaseEntity
{
    public Guid ReferrerId { get; set; } // User who referred
    public Guid ReferredUserId { get; set; } // User who was referred
    public Guid ReferralCodeId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public int PointsAwarded { get; set; } = 0;
    public Guid? FirstOrderId { get; set; } // First order of referred user

    // Navigation properties
    public User Referrer { get; set; } = null!;
    public User ReferredUser { get; set; } = null!;
    public ReferralCode ReferralCodeEntity { get; set; } = null!;
    public Order? FirstOrder { get; set; }
}

public enum ReferralStatus
{
    Pending, // User signed up but hasn't made purchase
    Completed, // User made first purchase
    Rewarded, // Referrer was rewarded
    Expired // Referral expired before completion
}

public class ReviewMedia : BaseEntity
{
    public Guid ReviewId { get; set; }
    public ReviewMediaType MediaType { get; set; }
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int FileSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Duration { get; set; } // For videos in seconds
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public Review Review { get; set; } = null!;
}

public enum ReviewMediaType
{
    Photo,
    Video
}

public class SharedWishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public string ShareCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}

public class SharedWishlistItem : BaseEntity
{
    public Guid SharedWishlistId { get; set; }
    public Guid ProductId { get; set; }
    public int Priority { get; set; } = 0; // 1 = High, 2 = Medium, 3 = Low
    public string Note { get; set; } = string.Empty;
    public bool IsPurchased { get; set; } = false;
    public Guid? PurchasedBy { get; set; }
    public DateTime? PurchasedAt { get; set; }

    // Navigation properties
    public SharedWishlist SharedWishlist { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public User? PurchasedByUser { get; set; }
}

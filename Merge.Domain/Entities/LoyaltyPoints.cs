namespace Merge.Domain.Entities;

public class LoyaltyAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public int PointsBalance { get; set; } = 0;
    public int LifetimePoints { get; set; } = 0;
    public Guid? TierId { get; set; }
    public DateTime? TierAchievedAt { get; set; }
    public DateTime? TierExpiresAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public LoyaltyTier? Tier { get; set; }
}

public class LoyaltyTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LoyaltyAccountId { get; set; }
    public int Points { get; set; } // Positive = earned, Negative = spent
    public LoyaltyTransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public Guid? ReviewId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired { get; set; } = false;

    // Navigation properties
    public User User { get; set; } = null!;
    public LoyaltyAccount LoyaltyAccount { get; set; } = null!;
    public Order? Order { get; set; }
    public Review? Review { get; set; }
}

public class LoyaltyTier : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Bronze, Silver, Gold, Platinum
    public string Description { get; set; } = string.Empty;
    public int MinimumPoints { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal PointsMultiplier { get; set; } = 1.0m;
    public string Benefits { get; set; } = string.Empty; // JSON or comma-separated
    public string Color { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public int Level { get; set; } // 1 = Bronze, 2 = Silver, etc.
    public bool IsActive { get; set; } = true;
}

public class LoyaltyRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LoyaltyTransactionType Type { get; set; }
    public int PointsAwarded { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public int? ExpiryDays { get; set; } // Points expire after X days
    public bool IsActive { get; set; } = true;
}

public enum LoyaltyTransactionType
{
    Purchase, // Points earned from purchase
    Review, // Points for writing review
    Referral, // Points for referring friend
    Signup, // Welcome bonus
    Birthday, // Birthday bonus
    Redeem, // Points redeemed for discount/reward
    Expired, // Points expired
    Adjustment, // Manual adjustment by admin
    Bonus, // Special promotion bonus
    Return // Points deducted on return
}

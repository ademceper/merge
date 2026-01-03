using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// LoyaltyTransaction Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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


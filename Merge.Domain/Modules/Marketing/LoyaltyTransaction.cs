using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// LoyaltyTransaction Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LoyaltyTransaction : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid LoyaltyAccountId { get; private set; }
    
    // ✅ BOLUM 1.6: Invariant validation - Points can be positive (earned) or negative (spent)
    private int _points;
    public int Points 
    { 
        get => _points; 
        private set 
        {
            _points = value;
        } 
    }
    
    public LoyaltyTransactionType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? OrderId { get; private set; }
    public Guid? ReviewId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsExpired { get; private set; } = false;

    // Navigation properties
    public User User { get; private set; } = null!;
    public LoyaltyAccount LoyaltyAccount { get; private set; } = null!;
    public Order? Order { get; private set; }
    public Review? Review { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private LoyaltyTransaction() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static LoyaltyTransaction Create(
        Guid userId,
        Guid loyaltyAccountId,
        int points,
        LoyaltyTransactionType type,
        string description,
        DateTime expiresAt,
        Guid? orderId = null,
        Guid? reviewId = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(loyaltyAccountId, nameof(loyaltyAccountId));
        Guard.AgainstNullOrEmpty(description, nameof(description));

        if (points == 0)
            throw new DomainException("Puan miktarı sıfır olamaz");

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Son kullanma tarihi gelecekte olmalıdır");

        return new LoyaltyTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LoyaltyAccountId = loyaltyAccountId,
            _points = points,
            Type = type,
            Description = description,
            ExpiresAt = expiresAt,
            OrderId = orderId,
            ReviewId = reviewId,
            IsExpired = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as expired
    public void MarkAsExpired()
    {
        if (IsExpired)
            return;

        IsExpired = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Check if expired
    public bool IsCurrentlyExpired()
    {
        return IsExpired || DateTime.UtcNow > ExpiresAt;
    }
}


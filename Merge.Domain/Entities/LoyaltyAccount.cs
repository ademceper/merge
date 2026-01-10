using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// LoyaltyAccount Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LoyaltyAccount : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    
    // ✅ BOLUM 1.6: Invariant validation - PointsBalance >= 0
    private int _pointsBalance = 0;
    public int PointsBalance 
    { 
        get => _pointsBalance; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(PointsBalance));
            _pointsBalance = value;
        }
    }
    
    // ✅ BOLUM 1.6: Invariant validation - LifetimePoints >= 0
    private int _lifetimePoints = 0;
    public int LifetimePoints 
    { 
        get => _lifetimePoints; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(LifetimePoints));
            _lifetimePoints = value;
        }
    }
    
    public Guid? TierId { get; private set; }
    public DateTime? TierAchievedAt { get; private set; }
    public DateTime? TierExpiresAt { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public LoyaltyTier? Tier { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private LoyaltyAccount() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static LoyaltyAccount Create(Guid userId)
    {
        Guard.AgainstDefault(userId, nameof(userId));

        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _pointsBalance = 0,
            _lifetimePoints = 0,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - LoyaltyAccountCreatedEvent
        account.AddDomainEvent(new LoyaltyAccountCreatedEvent(account.Id, userId));

        return account;
    }

    // ✅ BOLUM 1.1: Domain Method - Add points
    public void AddPoints(int points, string? reason = null)
    {
        Guard.AgainstNegativeOrZero(points, nameof(points));

        _pointsBalance += points;
        _lifetimePoints += points;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PointsAddedEvent
        AddDomainEvent(new PointsAddedEvent(Id, UserId, points, _pointsBalance, reason));
    }

    // ✅ BOLUM 1.1: Domain Method - Deduct points
    public void DeductPoints(int points, string? reason = null)
    {
        Guard.AgainstNegativeOrZero(points, nameof(points));

        if (points > _pointsBalance)
            throw new DomainException($"Yetersiz puan. Mevcut: {_pointsBalance}, İstenen: {points}");

        _pointsBalance -= points;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PointsDeductedEvent
        AddDomainEvent(new PointsDeductedEvent(Id, UserId, points, _pointsBalance, reason));
    }

    // ✅ BOLUM 1.1: Domain Method - Update tier
    public void UpdateTier(Guid tierId, DateTime? tierExpiresAt = null)
    {
        Guard.AgainstDefault(tierId, nameof(tierId));

        var oldTierId = TierId;
        TierId = tierId;
        TierAchievedAt = DateTime.UtcNow;
        TierExpiresAt = tierExpiresAt;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - TierUpdatedEvent
        AddDomainEvent(new TierUpdatedEvent(Id, UserId, oldTierId, tierId));
    }

    // ✅ BOLUM 1.1: Domain Method - Clear tier
    public void ClearTier()
    {
        if (TierId == null) return;

        var oldTierId = TierId;
        TierId = null;
        TierAchievedAt = null;
        TierExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - TierClearedEvent
        AddDomainEvent(new TierClearedEvent(Id, UserId, oldTierId.Value));
    }
}

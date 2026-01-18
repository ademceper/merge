using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Marketing;

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
    public Guid UserId { get; private set; }
    
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

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public LoyaltyTier? Tier { get; private set; }

    private LoyaltyAccount() { }

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

        account.AddDomainEvent(new LoyaltyAccountCreatedEvent(account.Id, userId));

        return account;
    }

    public void AddPoints(int points, string? reason = null)
    {
        Guard.AgainstNegativeOrZero(points, nameof(points));

        _pointsBalance += points;
        _lifetimePoints += points;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PointsAddedEvent(Id, UserId, points, _pointsBalance, reason));
    }

    public void DeductPoints(int points, string? reason = null)
    {
        Guard.AgainstNegativeOrZero(points, nameof(points));

        if (points > _pointsBalance)
            throw new DomainException($"Yetersiz puan. Mevcut: {_pointsBalance}, İstenen: {points}");

        _pointsBalance -= points;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PointsDeductedEvent(Id, UserId, points, _pointsBalance, reason));
    }

    public void UpdateTier(Guid tierId, DateTime? tierExpiresAt = null)
    {
        Guard.AgainstDefault(tierId, nameof(tierId));

        var oldTierId = TierId;
        TierId = tierId;
        TierAchievedAt = DateTime.UtcNow;
        TierExpiresAt = tierExpiresAt;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TierUpdatedEvent(Id, UserId, oldTierId, tierId));
    }

    public void ClearTier()
    {
        if (TierId is null) return;

        var oldTierId = TierId;
        TierId = null;
        TierAchievedAt = null;
        TierExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TierClearedEvent(Id, UserId, oldTierId.Value));
    }
}

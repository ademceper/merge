using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// Referral Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Referral : BaseEntity, IAggregateRoot
{
    public Guid ReferrerId { get; private set; } // User who referred
    public Guid ReferredUserId { get; private set; } // User who was referred
    public Guid ReferralCodeId { get; private set; }
    public string ReferralCode { get; private set; } = string.Empty;
    public ReferralStatus Status { get; private set; } = ReferralStatus.Pending;
    public DateTime? CompletedAt { get; private set; }
    
    private int _pointsAwarded = 0;
    public int PointsAwarded 
    { 
        get => _pointsAwarded; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(PointsAwarded));
            _pointsAwarded = value;
        }
    }
    
    public Guid? FirstOrderId { get; private set; } // First order of referred user

    // Navigation properties
    public User Referrer { get; private set; } = null!;
    public User ReferredUser { get; private set; } = null!;
    public ReferralCode ReferralCodeEntity { get; private set; } = null!;
    public Order? FirstOrder { get; private set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private Referral() { }

    public static Referral Create(
        Guid referrerId,
        Guid referredUserId,
        Guid referralCodeId,
        string referralCode)
    {
        Guard.AgainstDefault(referrerId, nameof(referrerId));
        Guard.AgainstDefault(referredUserId, nameof(referredUserId));
        Guard.AgainstDefault(referralCodeId, nameof(referralCodeId));
        Guard.AgainstNullOrEmpty(referralCode, nameof(referralCode));

        if (referrerId == referredUserId)
            throw new DomainException("Kullanıcı kendini referans edemez");

        var referral = new Referral
        {
            Id = Guid.NewGuid(),
            ReferrerId = referrerId,
            ReferredUserId = referredUserId,
            ReferralCodeId = referralCodeId,
            ReferralCode = referralCode,
            Status = ReferralStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        referral.AddDomainEvent(new ReferralCreatedEvent(referral.Id, referrerId, referredUserId, referralCode));

        return referral;
    }

    public void Complete(Guid firstOrderId, int pointsAwarded)
    {
        if (Status != ReferralStatus.Pending)
            throw new DomainException("Sadece bekleyen referanslar tamamlanabilir");

        Guard.AgainstDefault(firstOrderId, nameof(firstOrderId));
        Guard.AgainstNegative(pointsAwarded, nameof(pointsAwarded));

        Status = ReferralStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        FirstOrderId = firstOrderId;
        PointsAwarded = pointsAwarded;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReferralCompletedEvent(Id, ReferrerId, ReferredUserId, PointsAwarded, firstOrderId));
    }

    public void Expire()
    {
        if (Status != ReferralStatus.Pending)
            throw new DomainException("Sadece bekleyen referanslar süresi dolabilir");

        Status = ReferralStatus.Expired;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReferralExpiredEvent(Id, ReferrerId, ReferredUserId));
    }

    public void MarkAsRewarded()
    {
        if (Status != ReferralStatus.Completed)
            throw new DomainException("Sadece tamamlanmış referanslar ödüllendirilebilir");

        Status = ReferralStatus.Rewarded;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReferralRewardedEvent(Id, ReferrerId, PointsAwarded));
    }
}


using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// ReferralCode Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ReferralCode : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.6: Invariant validation - UsageCount >= 0
    private int _usageCount = 0;
    public int UsageCount 
    { 
        get => _usageCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(UsageCount));
            _usageCount = value;
        }
    }
    
    // ✅ BOLUM 1.6: Invariant validation - MaxUsage >= 0
    private int _maxUsage = 0; // 0 = unlimited
    public int MaxUsage 
    { 
        get => _maxUsage; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(MaxUsage));
            _maxUsage = value;
        }
    }
    
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    // ✅ BOLUM 1.6: Invariant validation - PointsReward >= 0
    private int _pointsReward = 100;
    public int PointsReward 
    { 
        get => _pointsReward; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(PointsReward));
            _pointsReward = value;
        }
    }
    
    // ✅ BOLUM 1.6: Invariant validation - DiscountPercentage >= 0 && <= 100
    private decimal _discountPercentage = 10;
    public decimal DiscountPercentage 
    { 
        get => _discountPercentage; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(DiscountPercentage));
            if (value > 100)
                throw new DomainException("İndirim yüzdesi %100'den fazla olamaz");
            _discountPercentage = value;
        }
    }

    // Navigation properties
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ReferralCode() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static ReferralCode Create(
        Guid userId,
        string code,
        int maxUsage = 0,
        DateTime? expiresAt = null,
        int pointsReward = 100,
        decimal discountPercentage = 10)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(code, nameof(code));
        Guard.AgainstNegative(maxUsage, nameof(maxUsage));
        Guard.AgainstNegative(pointsReward, nameof(pointsReward));
        Guard.AgainstNegative(discountPercentage, nameof(discountPercentage));

        if (discountPercentage > 100)
            throw new DomainException("İndirim yüzdesi %100'den fazla olamaz");

        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            throw new DomainException("Son kullanma tarihi gelecekte olmalıdır");

        var referralCode = new ReferralCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = code.ToUpperInvariant(),
            _maxUsage = maxUsage,
            ExpiresAt = expiresAt,
            _pointsReward = pointsReward,
            _discountPercentage = discountPercentage,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - ReferralCodeCreatedEvent
        referralCode.AddDomainEvent(new ReferralCodeCreatedEvent(referralCode.Id, userId, referralCode.Code));

        return referralCode;
    }

    // ✅ BOLUM 1.1: Domain Method - Increment usage
    public void IncrementUsage()
    {
        if (!IsActive)
            throw new DomainException("Referans kodu aktif değil");

        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
            throw new DomainException("Referans kodu süresi dolmuş");

        if (_maxUsage > 0 && _usageCount >= _maxUsage)
            throw new DomainException("Referans kodu kullanım limitine ulaşıldı");

        _usageCount++;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ReferralCodeUsedEvent
        AddDomainEvent(new ReferralCodeUsedEvent(Id, UserId, Code, _usageCount, _maxUsage));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ReferralCodeActivatedEvent
        AddDomainEvent(new ReferralCodeActivatedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ReferralCodeDeactivatedEvent
        AddDomainEvent(new ReferralCodeDeactivatedEvent(Id, Code));
    }

    // ✅ BOLUM 1.1: Domain Method - Check if valid
    public bool IsValid()
    {
        if (!IsActive)
            return false;

        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
            return false;

        if (_maxUsage > 0 && _usageCount >= _maxUsage)
            return false;

        return true;
    }

    // ✅ BOLUM 1.1: Domain Method - Update discount percentage
    public void UpdateDiscountPercentage(decimal discountPercentage)
    {
        Guard.AgainstNegative(discountPercentage, nameof(discountPercentage));
        if (discountPercentage > 100)
            throw new DomainException("İndirim yüzdesi %100'den fazla olamaz");

        DiscountPercentage = discountPercentage;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update points reward
    public void UpdatePointsReward(int pointsReward)
    {
        Guard.AgainstNegative(pointsReward, nameof(pointsReward));
        PointsReward = pointsReward;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

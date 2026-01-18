using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// ReferralCode Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ReferralCode : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    
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

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private ReferralCode() { }

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

        referralCode.AddDomainEvent(new ReferralCodeCreatedEvent(referralCode.Id, userId, referralCode.Code));

        return referralCode;
    }

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

        AddDomainEvent(new ReferralCodeUsedEvent(Id, UserId, Code, _usageCount, _maxUsage));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReferralCodeActivatedEvent(Id, Code));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReferralCodeDeactivatedEvent(Id, Code));
    }

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

    public void UpdateDiscountPercentage(decimal discountPercentage)
    {
        Guard.AgainstNegative(discountPercentage, nameof(discountPercentage));
        if (discountPercentage > 100)
            throw new DomainException("İndirim yüzdesi %100'den fazla olamaz");

        DiscountPercentage = discountPercentage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePointsReward(int pointsReward)
    {
        Guard.AgainstNegative(pointsReward, nameof(pointsReward));
        PointsReward = pointsReward;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReferralCodeDeletedEvent(Id, UserId, Code));
    }
}

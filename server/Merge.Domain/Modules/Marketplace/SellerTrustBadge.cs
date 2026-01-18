using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// SellerTrustBadge Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SellerTrustBadge : BaseEntity
{
    public Guid SellerId { get; private set; }
    public Guid TrustBadgeId { get; private set; }
    public DateTime AwardedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    private bool _isActive = true;
    public bool IsActive 
    { 
        get => _isActive; 
        private set => _isActive = value; 
    }
    public string? AwardReason { get; private set; }
    
    // Navigation properties
    public SellerProfile Seller { get; private set; } = null!;
    public TrustBadge TrustBadge { get; private set; } = null!;

    private SellerTrustBadge() { }

    public static SellerTrustBadge Create(
        Guid sellerId,
        Guid trustBadgeId,
        DateTime? awardedAt = null,
        DateTime? expiresAt = null,
        string? awardReason = null)
    {
        Guard.AgainstDefault(sellerId, nameof(sellerId));
        Guard.AgainstDefault(trustBadgeId, nameof(trustBadgeId));

        if (expiresAt.HasValue && awardedAt.HasValue && expiresAt.Value <= awardedAt.Value)
        {
            throw new DomainException("Expiry date must be after award date");
        }

        return new SellerTrustBadge
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            TrustBadgeId = trustBadgeId,
            AwardedAt = awardedAt ?? DateTime.UtcNow,
            ExpiresAt = expiresAt,
            _isActive = true,
            AwardReason = awardReason,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Activate()
    {
        if (_isActive) return;
        _isActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!_isActive) return;
        _isActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateExpiryDate(DateTime? newExpiryDate)
    {
        if (newExpiryDate.HasValue && newExpiryDate.Value <= AwardedAt)
        {
            throw new DomainException("Expiry date must be after award date");
        }
        ExpiresAt = newExpiryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAwardReason(string? newReason)
    {
        AwardReason = newReason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAwardedAt(DateTime newAwardedAt)
    {
        if (ExpiresAt.HasValue && ExpiresAt.Value <= newAwardedAt)
        {
            throw new DomainException("Expiry date must be after award date");
        }
        AwardedAt = newAwardedAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}


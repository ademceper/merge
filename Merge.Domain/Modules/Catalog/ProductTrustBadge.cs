using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Marketplace;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductTrustBadge Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductTrustBadge : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
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
    public Product Product { get; private set; } = null!;
    public TrustBadge TrustBadge { get; private set; } = null!;
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ProductTrustBadge() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static ProductTrustBadge Create(
        Guid productId,
        Guid trustBadgeId,
        DateTime? awardedAt = null,
        DateTime? expiresAt = null,
        string? awardReason = null)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstDefault(trustBadgeId, nameof(trustBadgeId));
        
        if (expiresAt.HasValue && awardedAt.HasValue && expiresAt.Value <= awardedAt.Value)
        {
            throw new DomainException("Expiry date must be after award date");
        }
        
        return new ProductTrustBadge
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            TrustBadgeId = trustBadgeId,
            AwardedAt = awardedAt ?? DateTime.UtcNow,
            ExpiresAt = expiresAt,
            _isActive = true,
            AwardReason = awardReason,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    // ✅ BOLUM 1.1: Domain Method - Activate badge
    public void Activate()
    {
        if (_isActive) return;
        
        _isActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // ✅ BOLUM 1.1: Domain Method - Deactivate badge
    public void Deactivate()
    {
        if (!_isActive) return;
        
        _isActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update expiry date
    public void UpdateExpiryDate(DateTime? newExpiryDate)
    {
        if (newExpiryDate.HasValue && newExpiryDate.Value <= AwardedAt)
        {
            throw new DomainException("Expiry date must be after award date");
        }
        
        ExpiresAt = newExpiryDate;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // ✅ BOLUM 1.1: Domain Method - Update award reason
    public void UpdateAwardReason(string? newReason)
    {
        AwardReason = newReason;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update awarded date
    public void UpdateAwardedAt(DateTime newAwardedAt)
    {
        if (ExpiresAt.HasValue && ExpiresAt.Value <= newAwardedAt)
        {
            throw new DomainException("Expiry date must be after award date");
        }
        AwardedAt = newAwardedAt;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // ✅ BOLUM 1.1: Domain Method - Check if expired
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
    
    // ✅ BOLUM 1.1: Domain Method - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}


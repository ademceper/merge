using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Marketplace;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductTrustBadge Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ProductTrustBadge : BaseEntity
{
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
    
    private static class ValidationConstants
    {
        public const int MaxAwardReasonLength = 500;
    }

    private string? _awardReason;
    public string? AwardReason 
    { 
        get => _awardReason; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstLength(value, ValidationConstants.MaxAwardReasonLength, nameof(AwardReason));
            }
            _awardReason = value;
        } 
    }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Product Product { get; private set; } = null!;
    public TrustBadge TrustBadge { get; private set; } = null!;
    
    private ProductTrustBadge() { }
    
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
        
        if (!string.IsNullOrEmpty(awardReason))
        {
            Guard.AgainstLength(awardReason, ValidationConstants.MaxAwardReasonLength, nameof(awardReason));
        }
        
        var badge = new ProductTrustBadge
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            TrustBadgeId = trustBadgeId,
            AwardedAt = awardedAt ?? DateTime.UtcNow,
            ExpiresAt = expiresAt,
            _isActive = true,
            _awardReason = awardReason,
            CreatedAt = DateTime.UtcNow
        };
        
        badge.ValidateInvariants();
        
        return badge;
    }
    
    public void Activate()
    {
        if (_isActive) return;
        
        _isActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void Deactivate()
    {
        if (!_isActive) return;
        
        _isActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void UpdateExpiryDate(DateTime? newExpiryDate)
    {
        if (newExpiryDate.HasValue && newExpiryDate.Value <= AwardedAt)
        {
            throw new DomainException("Expiry date must be after award date");
        }
        
        ExpiresAt = newExpiryDate;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void UpdateAwardReason(string? newReason)
    {
        if (!string.IsNullOrEmpty(newReason))
        {
            Guard.AgainstLength(newReason, ValidationConstants.MaxAwardReasonLength, nameof(newReason));
        }
        AwardReason = newReason;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void UpdateAwardedAt(DateTime newAwardedAt)
    {
        if (ExpiresAt.HasValue && ExpiresAt.Value <= newAwardedAt)
        {
            throw new DomainException("Expiry date must be after award date");
        }
        AwardedAt = newAwardedAt;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
    
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    private void ValidateInvariants()
    {
        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");

        if (Guid.Empty == TrustBadgeId)
            throw new DomainException("Trust badge ID boş olamaz");

        if (ExpiresAt.HasValue && ExpiresAt.Value <= AwardedAt)
            throw new DomainException("Expiry date must be after award date");
    }
}


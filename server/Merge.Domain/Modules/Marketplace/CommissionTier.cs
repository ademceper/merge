using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// CommissionTier Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class CommissionTier : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public decimal MinSales { get; private set; } = 0; // Minimum sales to qualify for this tier
    public decimal MaxSales { get; private set; } = decimal.MaxValue;
    public decimal CommissionRate { get; private set; } // Percentage
    public decimal PlatformFeeRate { get; private set; } = 0; // Percentage
    public bool IsActive { get; private set; } = true;
    public int Priority { get; private set; } = 0; // Higher priority tiers checked first

    private CommissionTier() { }

    public static CommissionTier Create(
        string name,
        decimal minSales,
        decimal maxSales,
        decimal commissionRate,
        decimal platformFeeRate = 0,
        int priority = 0)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNegative(minSales, nameof(minSales));
        Guard.AgainstNegative(maxSales, nameof(maxSales));
        Guard.AgainstNegative(commissionRate, nameof(commissionRate));
        Guard.AgainstNegative(platformFeeRate, nameof(platformFeeRate));
        Guard.AgainstNegative(priority, nameof(priority));

        if (maxSales < minSales)
            throw new DomainException("Maksimum satış minimum satıştan küçük olamaz");

        if (commissionRate > 100)
            throw new DomainException("Komisyon oranı %100'den fazla olamaz");

        if (platformFeeRate > 100)
            throw new DomainException("Platform ücreti oranı %100'den fazla olamaz");

        return new CommissionTier
        {
            Id = Guid.NewGuid(),
            Name = name,
            MinSales = minSales,
            MaxSales = maxSales,
            CommissionRate = commissionRate,
            PlatformFeeRate = platformFeeRate,
            IsActive = true,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string? name = null,
        decimal? minSales = null,
        decimal? maxSales = null,
        decimal? commissionRate = null,
        decimal? platformFeeRate = null,
        int? priority = null)
    {
        if (name != null)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Name = name;
        }

        var newMinSales = minSales ?? MinSales;
        var newMaxSales = maxSales ?? MaxSales;
        var newCommissionRate = commissionRate ?? CommissionRate;
        var newPlatformFeeRate = platformFeeRate ?? PlatformFeeRate;
        var newPriority = priority ?? Priority;

        if (newMaxSales < newMinSales)
            throw new DomainException("Maksimum satış minimum satıştan küçük olamaz");

        if (newCommissionRate > 100)
            throw new DomainException("Komisyon oranı %100'den fazla olamaz");

        if (newPlatformFeeRate > 100)
            throw new DomainException("Platform ücreti oranı %100'den fazla olamaz");

        if (minSales.HasValue)
            MinSales = newMinSales;

        if (maxSales.HasValue)
            MaxSales = newMaxSales;

        if (commissionRate.HasValue)
            CommissionRate = newCommissionRate;

        if (platformFeeRate.HasValue)
            PlatformFeeRate = newPlatformFeeRate;

        if (priority.HasValue)
            Priority = newPriority;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Tier zaten aktif");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Tier zaten pasif");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Tier zaten silinmiş");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool QualifiesForTier(decimal totalSales)
    {
        return IsActive && totalSales >= MinSales && totalSales <= MaxSales;
    }
}


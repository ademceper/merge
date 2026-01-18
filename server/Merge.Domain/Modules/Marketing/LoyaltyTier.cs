using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// LoyaltyTier Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LoyaltyTier : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty; // Bronze, Silver, Gold, Platinum
    public string Description { get; private set; } = string.Empty;
    
    private int _minimumPoints;
    public int MinimumPoints 
    { 
        get => _minimumPoints; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(MinimumPoints));
            _minimumPoints = value;
        } 
    }
    
    private decimal _discountPercentage = 0;
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
    
    private decimal _pointsMultiplier = 1.0m;
    public decimal PointsMultiplier 
    { 
        get => _pointsMultiplier; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(PointsMultiplier));
            _pointsMultiplier = value;
        } 
    }
    
    public string Benefits { get; private set; } = string.Empty; // JSON or comma-separated
    public string Color { get; private set; } = string.Empty;
    public string IconUrl { get; private set; } = string.Empty;
    
    private int _level;
    public int Level 
    { 
        get => _level; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Level));
            _level = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true;

    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Percentage DiscountPercentageValue => new Percentage(_discountPercentage);

    private LoyaltyTier() { }

    public static LoyaltyTier Create(
        string name,
        string description,
        int minimumPoints,
        Percentage discountPercentage,
        decimal pointsMultiplier,
        int level,
        string? benefits = null,
        string? color = null,
        string? iconUrl = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstNegative(minimumPoints, nameof(minimumPoints));
        Guard.AgainstNegativeOrZero(level, nameof(level));

        var tier = new LoyaltyTier
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            _minimumPoints = minimumPoints,
            _discountPercentage = discountPercentage.Value,
            _pointsMultiplier = pointsMultiplier,
            Level = level,
            Benefits = benefits ?? string.Empty,
            Color = color ?? string.Empty,
            IconUrl = iconUrl ?? string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        tier.AddDomainEvent(new LoyaltyTierCreatedEvent(tier.Id, tier.Name, tier.Level, tier.MinimumPoints));

        return tier;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LoyaltyTierActivatedEvent(Id, Name));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LoyaltyTierDeactivatedEvent(Id, Name));
    }

    public void UpdateDetails(
        string? name = null,
        string? description = null,
        int? minimumPoints = null,
        Percentage? discountPercentage = null,
        decimal? pointsMultiplier = null,
        string? benefits = null,
        string? color = null,
        string? iconUrl = null)
    {
        if (!string.IsNullOrEmpty(name))
            Name = name;

        if (!string.IsNullOrEmpty(description))
            Description = description;

        if (minimumPoints.HasValue)
            MinimumPoints = minimumPoints.Value;

        if (discountPercentage != null)
            DiscountPercentage = discountPercentage.Value;

        if (pointsMultiplier.HasValue)
            PointsMultiplier = pointsMultiplier.Value;

        if (benefits != null)
            Benefits = benefits;

        if (color != null)
            Color = color;

        if (iconUrl != null)
            IconUrl = iconUrl;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LoyaltyTierUpdatedEvent(Id, Name));
    }
}


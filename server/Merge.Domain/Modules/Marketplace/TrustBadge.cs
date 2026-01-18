using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// TrustBadge Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class TrustBadge : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string IconUrl { get; private set; } = string.Empty;
    public string BadgeType { get; private set; } = string.Empty; // Seller, Product, Order
    public string Criteria { get; private set; } = string.Empty; // JSON formatında kriterler
    private bool _isActive = true;
    public bool IsActive 
    { 
        get => _isActive; 
        private set => _isActive = value; 
    }
    public int DisplayOrder { get; private set; } = 0;
    public string? Color { get; private set; } // Hex color code

    private TrustBadge() { }

    public static TrustBadge Create(
        string name,
        string description,
        string iconUrl,
        string badgeType,
        string criteria,
        int displayOrder = 0,
        string? color = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(badgeType, nameof(badgeType));
        Guard.AgainstOutOfRange(displayOrder, 0, int.MaxValue, nameof(displayOrder));

        return new TrustBadge
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            IconUrl = iconUrl ?? string.Empty,
            BadgeType = badgeType,
            Criteria = criteria ?? string.Empty,
            _isActive = true,
            DisplayOrder = displayOrder,
            Color = color,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateName(string newName)
    {
        Guard.AgainstNullOrEmpty(newName, nameof(newName));
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string newDescription)
    {
        Description = newDescription ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateIconUrl(string newIconUrl)
    {
        IconUrl = newIconUrl ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBadgeType(string newBadgeType)
    {
        Guard.AgainstNullOrEmpty(newBadgeType, nameof(newBadgeType));
        BadgeType = newBadgeType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCriteria(string newCriteria)
    {
        Criteria = newCriteria ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
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

    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstOutOfRange(newDisplayOrder, 0, int.MaxValue, nameof(newDisplayOrder));
        DisplayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateColor(string? newColor)
    {
        Color = newColor;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}


using Merge.Domain.Common;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// NotificationPreference Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class NotificationPreference : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string NotificationType YASAK)
    public NotificationType NotificationType { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Channel YASAK)
    public NotificationChannel Channel { get; private set; }
    
    public bool IsEnabled { get; private set; } = true;
    
    private string? _customSettings;
    public string? CustomSettings 
    { 
        get => _customSettings; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 5000, nameof(CustomSettings));
            }
            _customSettings = value;
        }
    }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private NotificationPreference() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static NotificationPreference Create(
        Guid userId,
        NotificationType notificationType,
        NotificationChannel channel,
        bool isEnabled = true,
        string? customSettings = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        if (customSettings != null)
        {
            Guard.AgainstLength(customSettings, 5000, nameof(customSettings));
        }

        return new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NotificationType = notificationType,
            Channel = channel,
            IsEnabled = isEnabled,
            _customSettings = customSettings,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Update preference
    public void Update(bool isEnabled, string? customSettings = null)
    {
        if (customSettings != null)
        {
            Guard.AgainstLength(customSettings, 5000, nameof(customSettings));
        }

        IsEnabled = isEnabled;
        _customSettings = customSettings;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Enable preference
    public void Enable()
    {
        if (IsEnabled)
            return;

        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Disable preference
    public void Disable()
    {
        if (!IsEnabled)
            return;

        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Delete preference (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Tercih zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}


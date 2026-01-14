using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Notifications;

/// <summary>
/// NotificationPreference Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class NotificationPreference : BaseEntity, IAggregateRoot
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

        var preference = new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NotificationType = notificationType,
            Channel = channel,
            IsEnabled = isEnabled,
            _customSettings = customSettings,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - NotificationPreferenceCreatedEvent
        preference.AddDomainEvent(new NotificationPreferenceCreatedEvent(
            preference.Id, 
            userId, 
            notificationType, 
            channel, 
            isEnabled));

        return preference;
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

        // ✅ BOLUM 1.5: Domain Events - NotificationPreferenceUpdatedEvent
        AddDomainEvent(new NotificationPreferenceUpdatedEvent(Id, UserId, NotificationType, Channel, isEnabled));
    }

    // ✅ BOLUM 1.1: Domain Method - Enable preference
    public void Enable()
    {
        if (IsEnabled)
            return;

        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - NotificationPreferenceEnabledEvent
        AddDomainEvent(new NotificationPreferenceEnabledEvent(Id, UserId, NotificationType, Channel));
    }

    // ✅ BOLUM 1.1: Domain Method - Disable preference
    public void Disable()
    {
        if (!IsEnabled)
            return;

        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - NotificationPreferenceDisabledEvent
        AddDomainEvent(new NotificationPreferenceDisabledEvent(Id, UserId, NotificationType, Channel));
    }

    // ✅ BOLUM 1.1: Domain Method - Delete preference (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Tercih zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - NotificationPreferenceDeletedEvent
        AddDomainEvent(new NotificationPreferenceDeletedEvent(Id, UserId, NotificationType, Channel));
    }
}


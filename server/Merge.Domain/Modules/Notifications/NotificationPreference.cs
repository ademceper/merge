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
    public Guid UserId { get; private set; }
    
    public NotificationType NotificationType { get; private set; }
    
    public NotificationChannel Channel { get; private set; }
    
    public bool IsEnabled { get; private set; } = true;
    
    private string? _customSettings;
    public string? CustomSettings 
    { 
        get => _customSettings; 
        private set 
        {
            if (value is not null)
            {
                Guard.AgainstLength(value, 5000, nameof(CustomSettings));
            }
            _customSettings = value;
        }
    }
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    private NotificationPreference() { }

    public static NotificationPreference Create(
        Guid userId,
        NotificationType notificationType,
        NotificationChannel channel,
        bool isEnabled = true,
        string? customSettings = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        if (customSettings is not null)
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

        preference.AddDomainEvent(new NotificationPreferenceCreatedEvent(
            preference.Id, 
            userId, 
            notificationType, 
            channel, 
            isEnabled));

        return preference;
    }

    public void Update(bool isEnabled, string? customSettings = null)
    {
        if (customSettings is not null)
        {
            Guard.AgainstLength(customSettings, 5000, nameof(customSettings));
        }

        IsEnabled = isEnabled;
        _customSettings = customSettings;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationPreferenceUpdatedEvent(Id, UserId, NotificationType, Channel, isEnabled));
    }

    public void Enable()
    {
        if (IsEnabled)
            return;

        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationPreferenceEnabledEvent(Id, UserId, NotificationType, Channel));
    }

    public void Disable()
    {
        if (!IsEnabled)
            return;

        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationPreferenceDisabledEvent(Id, UserId, NotificationType, Channel));
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Tercih zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationPreferenceDeletedEvent(Id, UserId, NotificationType, Channel));
    }
}


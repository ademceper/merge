using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Enums;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// UserActivityLog Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (string ActivityType, EntityType YASAK)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserActivityLog : BaseEntity, IAggregateRoot
{
    public Guid? UserId { get; private set; } // Nullable for anonymous users
    
    public ActivityType ActivityType { get; private set; }
    
    public EntityType EntityType { get; private set; }
    public Guid? EntityId { get; private set; } // ID of the entity being acted upon
    public string Description { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public DeviceType DeviceType { get; private set; } = DeviceType.Other;
    public string Browser { get; private set; } = string.Empty;
    public string OS { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty; // City, Country
    public string Metadata { get; private set; } = string.Empty; // JSON data for additional context
    public int DurationMs { get; private set; } // Duration of action in milliseconds
    public bool WasSuccessful { get; private set; } = true;
    public string? ErrorMessage { get; private set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User? User { get; private set; }

    private UserActivityLog() { }

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    public static UserActivityLog Create(
        ActivityType activityType,
        EntityType entityType,
        string description,
        string ipAddress,
        string userAgent,
        Guid? userId = null,
        Guid? entityId = null,
        DeviceType deviceType = DeviceType.Other,
        string? browser = null,
        string? os = null,
        string? location = null,
        string? metadata = null,
        int durationMs = 0,
        bool wasSuccessful = true,
        string? errorMessage = null)
    {
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstLength(description, 1000, nameof(description));
        Guard.AgainstNullOrEmpty(ipAddress, nameof(ipAddress));
        Guard.AgainstLength(ipAddress, 50, nameof(ipAddress));
        Guard.AgainstNullOrEmpty(userAgent, nameof(userAgent));
        Guard.AgainstLength(userAgent, 500, nameof(userAgent));

        if (durationMs < 0)
            throw new DomainException("Duration cannot be negative");
        
        if (!string.IsNullOrEmpty(browser))
        {
            Guard.AgainstLength(browser, 100, nameof(browser));
        }
        
        if (!string.IsNullOrEmpty(os))
        {
            Guard.AgainstLength(os, 100, nameof(os));
        }
        
        if (!string.IsNullOrEmpty(location))
        {
            Guard.AgainstLength(location, 200, nameof(location));
        }
        
        if (!string.IsNullOrEmpty(metadata))
        {
            Guard.AgainstLength(metadata, 2000, nameof(metadata));
        }
        
        if (!string.IsNullOrEmpty(errorMessage))
        {
            Guard.AgainstLength(errorMessage, 500, nameof(errorMessage));
        }

        var log = new UserActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActivityType = activityType,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceType = deviceType,
            Browser = browser ?? string.Empty,
            OS = os ?? string.Empty,
            Location = location ?? string.Empty,
            Metadata = metadata ?? string.Empty,
            DurationMs = durationMs,
            WasSuccessful = wasSuccessful,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow
        };

        log.AddDomainEvent(new UserActivityLogCreatedEvent(log.Id, userId, activityType, entityType, entityId));

        return log;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));
        Guard.AgainstLength(errorMessage, 500, nameof(errorMessage));
        WasSuccessful = false;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserActivityLogFailedEvent(Id, UserId, errorMessage));
    }

    public void UpdateDuration(int durationMs)
    {
        if (durationMs < 0)
            throw new DomainException("Duration cannot be negative");
        DurationMs = durationMs;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserActivityLogDurationUpdatedEvent(Id, UserId, durationMs));
    }
}


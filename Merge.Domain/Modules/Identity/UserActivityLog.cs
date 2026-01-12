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
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class UserActivityLog : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid? UserId { get; private set; } // Nullable for anonymous users
    public string ActivityType { get; private set; } = string.Empty; // Login, Logout, ViewProduct, AddToCart, Purchase, etc.
    public string EntityType { get; private set; } = string.Empty; // Product, Order, Cart, etc.
    public Guid? EntityId { get; private set; } // ID of the entity being acted upon
    public string Description { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public string DeviceType { get; private set; } = string.Empty; // Mobile, Desktop, Tablet
    public string Browser { get; private set; } = string.Empty;
    public string OS { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty; // City, Country
    public string Metadata { get; private set; } = string.Empty; // JSON data for additional context
    public int DurationMs { get; private set; } // Duration of action in milliseconds
    public bool WasSuccessful { get; private set; } = true;
    public string? ErrorMessage { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User? User { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private UserActivityLog() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static UserActivityLog Create(
        string activityType,
        string entityType,
        string description,
        string ipAddress,
        string userAgent,
        Guid? userId = null,
        Guid? entityId = null,
        string? deviceType = null,
        string? browser = null,
        string? os = null,
        string? location = null,
        string? metadata = null,
        int durationMs = 0,
        bool wasSuccessful = true,
        string? errorMessage = null)
    {
        Guard.AgainstNullOrEmpty(activityType, nameof(activityType));
        Guard.AgainstNullOrEmpty(entityType, nameof(entityType));
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstNullOrEmpty(ipAddress, nameof(ipAddress));
        Guard.AgainstNullOrEmpty(userAgent, nameof(userAgent));

        if (durationMs < 0)
            throw new DomainException("Duration cannot be negative");

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
            DeviceType = deviceType ?? string.Empty,
            Browser = browser ?? string.Empty,
            OS = os ?? string.Empty,
            Location = location ?? string.Empty,
            Metadata = metadata ?? string.Empty,
            DurationMs = durationMs,
            WasSuccessful = wasSuccessful,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - Add domain event
        log.AddDomainEvent(new UserActivityLogCreatedEvent(log.Id, userId, activityType, entityType, entityId));

        return log;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as failed
    public void MarkAsFailed(string errorMessage)
    {
        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));
        WasSuccessful = false;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update duration
    public void UpdateDuration(int durationMs)
    {
        if (durationMs < 0)
            throw new DomainException("Duration cannot be negative");
        DurationMs = durationMs;
        UpdatedAt = DateTime.UtcNow;
    }
}


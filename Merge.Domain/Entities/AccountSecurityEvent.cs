using Merge.Domain.Enums;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// AccountSecurityEvent Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class AccountSecurityEvent : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string EventType YASAK)
    public SecurityEventType EventType { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Severity YASAK)
    public SecurityEventSeverity Severity { get; private set; } = SecurityEventSeverity.Info;
    
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Location { get; private set; } // Country/City
    public string? DeviceFingerprint { get; private set; }
    public bool IsSuspicious { get; private set; } = false;
    public string? Details { get; private set; } // JSON for additional details
    public bool RequiresAction { get; private set; } = false;
    public string? ActionTaken { get; private set; } // Account locked, Password reset required, etc.
    public Guid? ActionTakenByUserId { get; private set; } // Admin who took action
    public DateTime? ActionTakenAt { get; private set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public User? ActionTakenBy { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private AccountSecurityEvent() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static AccountSecurityEvent Create(
        Guid userId,
        SecurityEventType eventType,
        SecurityEventSeverity severity = SecurityEventSeverity.Info,
        string? ipAddress = null,
        string? userAgent = null,
        string? location = null,
        string? deviceFingerprint = null,
        bool isSuspicious = false,
        string? details = null,
        bool requiresAction = false)
    {
        Guard.AgainstDefault(userId, nameof(userId));

        var securityEvent = new AccountSecurityEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            Severity = severity,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Location = location,
            DeviceFingerprint = deviceFingerprint,
            IsSuspicious = isSuspicious,
            Details = details,
            RequiresAction = requiresAction,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events
        securityEvent.AddDomainEvent(new AccountSecurityEventCreatedEvent(
            securityEvent.Id,
            userId,
            eventType,
            severity,
            isSuspicious,
            requiresAction));

        return securityEvent;
    }

    // ✅ BOLUM 1.1: Domain Method - Take action
    public void TakeAction(Guid actionTakenByUserId, string action, string? notes = null)
    {
        Guard.AgainstDefault(actionTakenByUserId, nameof(actionTakenByUserId));
        Guard.AgainstNullOrEmpty(action, nameof(action));

        if (ActionTaken != null)
            throw new DomainException("Action has already been taken for this event");

        ActionTaken = action;
        ActionTakenByUserId = actionTakenByUserId;
        ActionTakenAt = DateTime.UtcNow;
        RequiresAction = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new AccountSecurityEventActionTakenEvent(Id, actionTakenByUserId, action));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as suspicious
    public void MarkAsSuspicious()
    {
        if (IsSuspicious) return;
        IsSuspicious = true;
        if (Severity == SecurityEventSeverity.Info)
            Severity = SecurityEventSeverity.Warning;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update severity
    public void UpdateSeverity(SecurityEventSeverity severity)
    {
        Severity = severity;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Require action
    public void RequireAction()
    {
        if (RequiresAction) return;
        RequiresAction = true;
        UpdatedAt = DateTime.UtcNow;
    }
}


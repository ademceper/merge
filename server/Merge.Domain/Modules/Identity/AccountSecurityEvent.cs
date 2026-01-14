using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Identity;

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
    public User User { get; private set; } = null!;
    public User? ActionTakenBy { get; private set; }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation - Remove domain event
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

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
        
        if (!string.IsNullOrEmpty(ipAddress))
        {
            Guard.AgainstLength(ipAddress, 50, nameof(ipAddress));
        }
        
        if (!string.IsNullOrEmpty(userAgent))
        {
            Guard.AgainstLength(userAgent, 500, nameof(userAgent));
        }
        
        if (!string.IsNullOrEmpty(location))
        {
            Guard.AgainstLength(location, 200, nameof(location));
        }
        
        if (!string.IsNullOrEmpty(deviceFingerprint))
        {
            Guard.AgainstLength(deviceFingerprint, 256, nameof(deviceFingerprint));
        }
        
        if (!string.IsNullOrEmpty(details))
        {
            Guard.AgainstLength(details, 2000, nameof(details));
        }

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
        Guard.AgainstLength(action, 200, nameof(action));
        
        if (!string.IsNullOrEmpty(notes))
        {
            Guard.AgainstLength(notes, 1000, nameof(notes));
        }

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

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new AccountSecurityEventMarkedAsSuspiciousEvent(Id, UserId, EventType, Severity));
    }

    // ✅ BOLUM 1.1: Domain Method - Update severity
    public void UpdateSeverity(SecurityEventSeverity severity)
    {
        if (Severity == severity) return;
        
        Severity = severity;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new AccountSecurityEventSeverityUpdatedEvent(Id, UserId, EventType, severity));
    }

    // ✅ BOLUM 1.1: Domain Method - Require action
    public void RequireAction()
    {
        if (RequiresAction) return;
        RequiresAction = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new AccountSecurityEventRequiresActionEvent(Id, UserId, EventType, Severity));
    }
}


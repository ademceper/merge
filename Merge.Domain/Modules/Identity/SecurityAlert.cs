using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// SecurityAlert Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (string AlertType YASAK)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SecurityAlert : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid? UserId { get; private set; } // Null for system-wide alerts
    
    // ✅ BOLUM 1.2: Enum kullanımı (string AlertType YASAK)
    public AlertType AlertType { get; private set; }
    // ✅ BOLUM 1.2: Enum kullanımı (string Severity YASAK)
    public AlertSeverity Severity { get; private set; } = AlertSeverity.Medium;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    // ✅ BOLUM 1.2: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public AlertStatus Status { get; private set; } = AlertStatus.New;
    public Guid? AcknowledgedByUserId { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public Guid? ResolvedByUserId { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public string? Metadata { get; private set; } // JSON for additional data

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User? User { get; private set; }
    public User? AcknowledgedBy { get; private set; }
    public User? ResolvedBy { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SecurityAlert() { }

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

    // ✅ BOLUM 1.1: Factory Method with validation
    public static SecurityAlert Create(
        AlertType alertType,
        string title,
        string description,
        AlertSeverity severity = AlertSeverity.Medium,
        Guid? userId = null,
        string? metadata = null)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstLength(title, 200, nameof(title));
        Guard.AgainstNullOrEmpty(description, nameof(description));
        Guard.AgainstLength(description, 2000, nameof(description));
        
        if (!string.IsNullOrEmpty(metadata))
        {
            Guard.AgainstLength(metadata, 2000, nameof(metadata));
        }

        var alert = new SecurityAlert
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AlertType = alertType,
            Title = title,
            Description = description,
            Severity = severity,
            Status = AlertStatus.New,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events
        alert.AddDomainEvent(new SecurityAlertCreatedEvent(alert.Id, alertType, alert.Severity));

        return alert;
    }

    // ✅ BOLUM 1.1: Domain Method - Acknowledge alert
    public void Acknowledge(Guid acknowledgedByUserId)
    {
        Guard.AgainstDefault(acknowledgedByUserId, nameof(acknowledgedByUserId));

        if (Status != AlertStatus.New)
            throw new DomainException("Only new alerts can be acknowledged");

        AcknowledgedByUserId = acknowledgedByUserId;
        AcknowledgedAt = DateTime.UtcNow;
        Status = AlertStatus.Acknowledged;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new SecurityAlertAcknowledgedEvent(Id, acknowledgedByUserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Resolve alert
    public void Resolve(Guid resolvedByUserId, string? resolutionNotes = null)
    {
        Guard.AgainstDefault(resolvedByUserId, nameof(resolvedByUserId));

        if (Status == AlertStatus.Resolved)
            throw new DomainException("Alert is already resolved");
        
        if (!string.IsNullOrEmpty(resolutionNotes))
        {
            Guard.AgainstLength(resolutionNotes, 1000, nameof(resolutionNotes));
        }

        ResolvedByUserId = resolvedByUserId;
        ResolvedAt = DateTime.UtcNow;
        ResolutionNotes = resolutionNotes;
        Status = AlertStatus.Resolved;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new SecurityAlertResolvedEvent(Id, resolvedByUserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Update severity
    public void UpdateSeverity(AlertSeverity severity)
    {
        if (Severity == severity) return;
        
        Severity = severity;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new SecurityAlertSeverityUpdatedEvent(Id, severity));
    }
}


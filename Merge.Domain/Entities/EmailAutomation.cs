using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// EmailAutomation Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailAutomation : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public EmailAutomationType Type { get; private set; } = EmailAutomationType.WelcomeSeries;
    public bool IsActive { get; private set; } = true;
    public Guid TemplateId { get; private set; }
    public EmailTemplate Template { get; private set; } = null!;
    
    private int _delayHours = 0;
    public int DelayHours 
    { 
        get => _delayHours; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(DelayHours));
            _delayHours = value;
        }
    }
    
    public string? TriggerConditions { get; private set; } // JSON object defining when to trigger
    
    private int _totalTriggered = 0;
    public int TotalTriggered 
    { 
        get => _totalTriggered; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(TotalTriggered));
            _totalTriggered = value;
        }
    }
    
    private int _totalSent = 0;
    public int TotalSent 
    { 
        get => _totalSent; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(TotalSent));
            _totalSent = value;
        }
    }
    
    public DateTime? LastTriggeredAt { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private EmailAutomation() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static EmailAutomation Create(
        string name,
        string description,
        EmailAutomationType type,
        Guid templateId,
        int delayHours = 0,
        string? triggerConditions = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstDefault(templateId, nameof(templateId));
        Guard.AgainstNegative(delayHours, nameof(delayHours));
        Guard.AgainstLength(name, 200, nameof(name));

        var automation = new EmailAutomation
        {
            Name = name,
            Description = description ?? string.Empty,
            Type = type,
            TemplateId = templateId,
            DelayHours = delayHours,
            TriggerConditions = triggerConditions,
            IsActive = true
        };

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        automation.AddDomainEvent(new EmailAutomationCreatedEvent(automation.Id, name, type));

        return automation;
    }

    // ✅ BOLUM 1.1: Domain Method - Update details
    public void UpdateDetails(
        string? name = null,
        string? description = null,
        EmailAutomationType? type = null,
        Guid? templateId = null,
        int? delayHours = null,
        string? triggerConditions = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Guard.AgainstLength(name, 200, nameof(name));
            Name = name;
        }

        if (description != null)
            Description = description;

        if (type.HasValue)
            Type = type.Value;

        if (templateId.HasValue)
        {
            Guard.AgainstDefault(templateId.Value, nameof(templateId));
            TemplateId = templateId.Value;
        }

        if (delayHours.HasValue)
            DelayHours = delayHours.Value;

        if (triggerConditions != null)
            TriggerConditions = triggerConditions;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailAutomationUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailAutomationActivatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailAutomationDeactivatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Record trigger
    public void RecordTrigger()
    {
        TotalTriggered++;
        LastTriggeredAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Record sent
    public void RecordSent()
    {
        TotalSent++;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        IsActive = false;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailAutomationDeletedEvent(Id, Name));
    }
}


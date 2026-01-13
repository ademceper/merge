using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// FraudDetectionRule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class FraudDetectionRule : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.2: Enum kullanımı (string RuleType YASAK)
    public FraudRuleType RuleType { get; private set; }
    
    public string Conditions { get; private set; } = string.Empty; // JSON string for rule conditions
    
    // ✅ BOLUM 1.6: Invariant validation - RiskScore 0-100 arası
    private int _riskScore = 0;
    public int RiskScore 
    { 
        get => _riskScore; 
        private set 
        {
            Guard.AgainstOutOfRange(value, 0, 100, nameof(RiskScore));
            _riskScore = value;
        }
    }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Action YASAK)
    public FraudAction Action { get; private set; } = FraudAction.Flag;
    
    public bool IsActive { get; private set; } = true;
    
    // ✅ BOLUM 1.6: Invariant validation - Priority >= 0
    private int _priority = 0;
    public int Priority 
    { 
        get => _priority; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(Priority));
            _priority = value;
        }
    }
    
    public string? Description { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private FraudDetectionRule() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static FraudDetectionRule Create(
        string name,
        FraudRuleType ruleType,
        string conditions,
        int riskScore,
        FraudAction action = FraudAction.Flag,
        int priority = 0,
        string? description = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(conditions, nameof(conditions));
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));
        Guard.AgainstNegative(priority, nameof(priority));

        var rule = new FraudDetectionRule
        {
            Id = Guid.NewGuid(),
            Name = name,
            RuleType = ruleType,
            Conditions = conditions,
            _riskScore = riskScore,
            Action = action,
            _priority = priority,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleCreatedEvent
        rule.AddDomainEvent(new FraudDetectionRuleCreatedEvent(rule.Id, name, ruleType));

        return rule;
    }

    // ✅ BOLUM 1.1: Domain Method - Update name
    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Name = name;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleUpdatedEvent
        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Update conditions
    public void UpdateConditions(string conditions)
    {
        Guard.AgainstNullOrEmpty(conditions, nameof(conditions));
        Conditions = conditions;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleUpdatedEvent
        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Update risk score
    public void UpdateRiskScore(int riskScore)
    {
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));
        _riskScore = riskScore;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleUpdatedEvent
        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Update action
    public void UpdateAction(FraudAction action)
    {
        Action = action;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleUpdatedEvent
        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Update priority
    public void UpdatePriority(int priority)
    {
        Guard.AgainstNegative(priority, nameof(priority));
        _priority = priority;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleUpdatedEvent
        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Update description
    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleUpdatedEvent
        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Update rule type
    public void UpdateRuleType(FraudRuleType ruleType)
    {
        RuleType = ruleType;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleUpdatedEvent
        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate
    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleActivatedEvent
        AddDomainEvent(new FraudDetectionRuleActivatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleDeactivatedEvent
        AddDomainEvent(new FraudDetectionRuleDeactivatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - FraudDetectionRuleDeletedEvent
        AddDomainEvent(new FraudDetectionRuleDeletedEvent(Id, Name));
    }
}


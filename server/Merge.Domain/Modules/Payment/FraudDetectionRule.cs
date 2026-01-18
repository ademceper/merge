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
    public string Name { get; private set; } = string.Empty;
    
    public FraudRuleType RuleType { get; private set; }
    
    public string Conditions { get; private set; } = string.Empty; // JSON string for rule conditions
    
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
    
    public FraudAction Action { get; private set; } = FraudAction.Flag;
    
    public bool IsActive { get; private set; } = true;
    
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

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private FraudDetectionRule() { }

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

        rule.AddDomainEvent(new FraudDetectionRuleCreatedEvent(rule.Id, name, ruleType));

        return rule;
    }

    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Name = name;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    public void UpdateConditions(string conditions)
    {
        Guard.AgainstNullOrEmpty(conditions, nameof(conditions));
        Conditions = conditions;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    public void UpdateRiskScore(int riskScore)
    {
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));
        _riskScore = riskScore;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    public void UpdateAction(FraudAction action)
    {
        Action = action;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    public void UpdatePriority(int priority)
    {
        Guard.AgainstNegative(priority, nameof(priority));
        _priority = priority;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    public void UpdateRuleType(FraudRuleType ruleType)
    {
        RuleType = ruleType;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudDetectionRuleUpdatedEvent(Id, Name));
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new FraudDetectionRuleActivatedEvent(Id, Name));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new FraudDetectionRuleDeactivatedEvent(Id, Name));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new FraudDetectionRuleDeletedEvent(Id, Name));
    }
}


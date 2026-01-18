using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Analytics;

/// <summary>
/// DataQualityRule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot gerekli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class DataQualityRule : BaseAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string RuleType { get; private set; } = string.Empty; // Completeness, Accuracy, Consistency, Validity, Timeliness
    public string TargetEntity { get; private set; } = string.Empty; // Entity/Table name
    public string? FieldName { get; private set; } // Specific field to check
    public string? RuleExpression { get; private set; } // SQL or expression for validation
    public EntityStatus Status { get; private set; } = EntityStatus.Active;
    public int PassCount { get; private set; } = 0;
    public int FailCount { get; private set; } = 0;
    public decimal PassRate { get; private set; } = 100; // Percentage
    public DateTime? LastCheckAt { get; private set; }
    public string? Severity { get; private set; } = "Medium"; // Low, Medium, High, Critical

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private DataQualityRule() { }

    public static DataQualityRule Create(
        string name,
        string description,
        string ruleType,
        string targetEntity,
        string? fieldName = null,
        string? ruleExpression = null,
        string? severity = "Medium")
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(ruleType, nameof(ruleType));
        Guard.AgainstNullOrEmpty(targetEntity, nameof(targetEntity));

        var rule = new DataQualityRule
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            RuleType = ruleType,
            TargetEntity = targetEntity,
            FieldName = fieldName,
            RuleExpression = ruleExpression,
            Status = EntityStatus.Active,
            PassCount = 0,
            FailCount = 0,
            PassRate = 100,
            Severity = severity ?? "Medium",
            CreatedAt = DateTime.UtcNow
        };

        rule.AddDomainEvent(new DataQualityRuleCreatedEvent(
            rule.Id,
            name,
            ruleType,
            targetEntity));

        return rule;
    }

    public void UpdateStatistics(int passCount, int failCount, DateTime lastCheckAt)
    {
        if (passCount < 0)
            throw new DomainException("Geçen kayıt sayısı negatif olamaz");
        if (failCount < 0)
            throw new DomainException("Başarısız kayıt sayısı negatif olamaz");

        PassCount = passCount;
        FailCount = failCount;
        LastCheckAt = lastCheckAt;

        var total = passCount + failCount;
        PassRate = total > 0 ? (decimal)passCount / total * 100 : 100;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == EntityStatus.Active)
            return;

        Status = EntityStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DataQualityRuleActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (Status == EntityStatus.Inactive)
            return;

        Status = EntityStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DataQualityRuleDeactivatedEvent(Id));
    }

    public void UpdateRuleExpression(string ruleExpression)
    {
        RuleExpression = ruleExpression;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DataQualityRuleDeletedEvent(Id));
    }
}


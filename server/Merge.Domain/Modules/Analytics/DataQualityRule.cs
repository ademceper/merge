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
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string RuleType { get; private set; } = string.Empty; // Completeness, Accuracy, Consistency, Validity, Timeliness
    public string TargetEntity { get; private set; } = string.Empty; // Entity/Table name
    public string? FieldName { get; private set; } // Specific field to check
    public string? RuleExpression { get; private set; } // SQL or expression for validation
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public EntityStatus Status { get; private set; } = EntityStatus.Active;
    public int PassCount { get; private set; } = 0;
    public int FailCount { get; private set; } = 0;
    public decimal PassRate { get; private set; } = 100; // Percentage
    public DateTime? LastCheckAt { get; private set; }
    public string? Severity { get; private set; } = "Medium"; // Low, Medium, High, Critical

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private DataQualityRule() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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

        // ✅ BOLUM 1.5: Domain Events - DataQualityRuleCreatedEvent yayınla
        rule.AddDomainEvent(new DataQualityRuleCreatedEvent(
            rule.Id,
            name,
            ruleType,
            targetEntity));

        return rule;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update statistics
    public void UpdateStatistics(int passCount, int failCount, DateTime lastCheckAt)
    {
        // ✅ BOLUM 1.6: Invariant Validation - PassCount >= 0, FailCount >= 0
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

    // ✅ BOLUM 1.1: Domain Logic - Activate rule
    public void Activate()
    {
        if (Status == EntityStatus.Active)
            return;

        Status = EntityStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - DataQualityRuleActivatedEvent yayınla
        AddDomainEvent(new DataQualityRuleActivatedEvent(Id));
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate rule
    public void Deactivate()
    {
        if (Status == EntityStatus.Inactive)
            return;

        Status = EntityStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - DataQualityRuleDeactivatedEvent yayınla
        AddDomainEvent(new DataQualityRuleDeactivatedEvent(Id));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update rule expression
    public void UpdateRuleExpression(string ruleExpression)
    {
        RuleExpression = ruleExpression;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - DataQualityRuleDeletedEvent yayınla
        AddDomainEvent(new DataQualityRuleDeletedEvent(Id));
    }
}


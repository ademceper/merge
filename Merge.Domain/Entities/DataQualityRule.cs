using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// DataQualityRule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class DataQualityRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // Completeness, Accuracy, Consistency, Validity, Timeliness
    public string TargetEntity { get; set; } = string.Empty; // Entity/Table name
    public string? FieldName { get; set; } // Specific field to check
    public string? RuleExpression { get; set; } // SQL or expression for validation
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    public int PassCount { get; set; } = 0;
    public int FailCount { get; set; } = 0;
    public decimal PassRate { get; set; } = 100; // Percentage
    public DateTime? LastCheckAt { get; set; }
    public string? Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
}


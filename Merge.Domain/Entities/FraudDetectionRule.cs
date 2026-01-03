namespace Merge.Domain.Entities;

/// <summary>
/// FraudDetectionRule Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class FraudDetectionRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // Order, Payment, Account, Behavior
    public string Conditions { get; set; } = string.Empty; // JSON string for rule conditions
    public int RiskScore { get; set; } = 0; // Risk score if rule matches (0-100)
    public string Action { get; set; } = "Flag"; // Flag, Block, Review, Alert
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0; // Higher priority rules checked first
    public string? Description { get; set; }
}


namespace Merge.Application.DTOs.ML;

/// <summary>
/// Partial update DTO for Fraud Detection Rule (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchFraudDetectionRuleDto
{
    public string? Name { get; init; }
    public string? RuleType { get; init; }
    public FraudRuleConditionsDto? Conditions { get; init; }
    public int? RiskScore { get; init; }
    public string? Action { get; init; }
    public bool? IsActive { get; init; }
    public int? Priority { get; init; }
    public string? Description { get; init; }
}

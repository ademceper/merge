namespace Merge.Application.DTOs.Content;

public class FraudDetectionRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    /// Typed DTO (Over-posting korumasi)
    public FraudRuleConditionsDto? Conditions { get; set; }
    public int RiskScore { get; set; }
    public string Action { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

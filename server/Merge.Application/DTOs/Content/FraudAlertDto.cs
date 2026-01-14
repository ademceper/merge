namespace Merge.Application.DTOs.Content;

public class FraudAlertDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? PaymentId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public List<Guid>? MatchedRules { get; set; }
    public DateTime CreatedAt { get; set; }
}

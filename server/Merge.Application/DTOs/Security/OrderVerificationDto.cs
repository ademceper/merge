using Merge.Domain.Enums;
namespace Merge.Application.DTOs.Security;

public class OrderVerificationDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string VerificationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? VerifiedByUserId { get; set; }
    public string? VerifiedByName { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }
    public string? VerificationMethod { get; set; }
    public bool RequiresManualReview { get; set; }
    public int RiskScore { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

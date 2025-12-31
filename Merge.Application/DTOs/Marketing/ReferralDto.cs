namespace Merge.Application.DTOs.Marketing;

public class ReferralDto
{
    public Guid Id { get; set; }
    public string ReferredUserEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int PointsAwarded { get; set; }
}

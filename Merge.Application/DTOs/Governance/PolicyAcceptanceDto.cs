namespace Merge.Application.DTOs.Governance;

public class PolicyAcceptanceDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public string PolicyTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AcceptedVersion { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime AcceptedAt { get; set; }
    public bool IsActive { get; set; }
}

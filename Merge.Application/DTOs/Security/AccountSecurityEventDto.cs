namespace Merge.Application.DTOs.Security;

public class AccountSecurityEventDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; }
    public string? DeviceFingerprint { get; set; }
    public bool IsSuspicious { get; set; }
    public Dictionary<string, object>? Details { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionTaken { get; set; }
    public Guid? ActionTakenByUserId { get; set; }
    public string? ActionTakenByName { get; set; }
    public DateTime? ActionTakenAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

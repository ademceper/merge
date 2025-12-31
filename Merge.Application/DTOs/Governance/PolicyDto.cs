namespace Merge.Application.DTOs.Governance;

public class PolicyDto
{
    public Guid Id { get; set; }
    public string PolicyType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool RequiresAcceptance { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public string? ChangeLog { get; set; }
    public string Language { get; set; } = string.Empty;
    public int AcceptanceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

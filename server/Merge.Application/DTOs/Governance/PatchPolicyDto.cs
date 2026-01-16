namespace Merge.Application.DTOs.Governance;

/// <summary>
/// Partial update DTO for Policy (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchPolicyDto
{
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? Version { get; init; }
    public bool? IsActive { get; init; }
    public bool? RequiresAcceptance { get; init; }
    public DateTime? EffectiveDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? ChangeLog { get; init; }
}

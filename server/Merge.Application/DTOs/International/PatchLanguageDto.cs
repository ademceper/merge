namespace Merge.Application.DTOs.International;

/// <summary>
/// Partial update DTO for Language (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchLanguageDto
{
    public string? Name { get; init; }
    public string? NativeName { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsRTL { get; init; }
    public string? FlagIcon { get; init; }
}

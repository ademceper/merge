namespace Merge.Application.DTOs.B2B;

/// <summary>
/// Partial update DTO for B2B User (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchB2BUserDto
{
    public string? EmployeeId { get; init; }
    public string? Department { get; init; }
    public string? JobTitle { get; init; }
    public string? Status { get; init; }
    public decimal? CreditLimit { get; init; }
    public B2BUserSettingsDto? Settings { get; init; }
}

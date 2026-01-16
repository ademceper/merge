namespace Merge.Application.DTOs.Organization;

/// <summary>
/// Partial update DTO for Organization (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchOrganizationDto
{
    public string? Name { get; init; }
    public string? LegalName { get; init; }
    public string? TaxNumber { get; init; }
    public string? RegistrationNumber { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    public string? Address { get; init; }
    public string? AddressLine2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? Status { get; init; }
    public OrganizationSettingsDto? Settings { get; init; }
}

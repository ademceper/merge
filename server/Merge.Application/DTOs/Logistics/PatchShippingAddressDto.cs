namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Partial update DTO for Shipping Address (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchShippingAddressDto
{
    public string? Label { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Phone { get; init; }
    public string? AddressLine1 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? AddressLine2 { get; init; }
    public bool? IsDefault { get; init; }
    public bool? IsActive { get; init; }
    public string? Instructions { get; init; }
}

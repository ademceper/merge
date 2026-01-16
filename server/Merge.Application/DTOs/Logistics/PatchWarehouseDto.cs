namespace Merge.Application.DTOs.Logistics;

/// <summary>
/// Partial update DTO for Warehouse (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchWarehouseDto
{
    public string? Name { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    public string? ContactPerson { get; init; }
    public string? ContactPhone { get; init; }
    public string? ContactEmail { get; init; }
    public int? Capacity { get; init; }
    public bool? IsActive { get; init; }
    public string? Description { get; init; }
}

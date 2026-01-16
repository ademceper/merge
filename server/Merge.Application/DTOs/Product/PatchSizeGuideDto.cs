namespace Merge.Application.DTOs.Product;

/// <summary>
/// Partial update DTO for Size Guide (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchSizeGuideDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Brand { get; init; }
    public string? Type { get; init; }
    public string? MeasurementUnit { get; init; }
    public List<CreateSizeGuideEntryDto>? Entries { get; init; }
}

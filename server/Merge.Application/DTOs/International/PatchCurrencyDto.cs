namespace Merge.Application.DTOs.International;

/// <summary>
/// Partial update DTO for Currency (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCurrencyDto
{
    public string? Name { get; init; }
    public string? Symbol { get; init; }
    public decimal? ExchangeRate { get; init; }
    public bool? IsActive { get; init; }
    public int? DecimalPlaces { get; init; }
    public string? Format { get; init; }
}

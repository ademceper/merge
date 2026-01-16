using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

/// <summary>
/// Partial update DTO for Exchange Rate (PATCH support)
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchExchangeRateDto
{
    [Range(0.0001, double.MaxValue, ErrorMessage = "Kur 0'dan büyük olmalıdır.")]
    public decimal? NewRate { get; init; }
    
    [StringLength(50)]
    public string? Source { get; init; }
}

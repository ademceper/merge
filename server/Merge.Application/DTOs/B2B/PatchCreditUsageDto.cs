using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

/// <summary>
/// Partial update DTO for Credit Usage (PATCH support)
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCreditUsageDto
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
    public decimal? Amount { get; init; }
}

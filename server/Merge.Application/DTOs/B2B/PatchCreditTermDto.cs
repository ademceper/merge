using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

/// <summary>
/// Partial update DTO for Credit Term (PATCH support)
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCreditTermDto
{
    public Guid? OrganizationId { get; init; }
    
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    public string? Name { get; init; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Ödeme günü en az 1 olmalıdır.")]
    public int? PaymentDays { get; init; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Kredi limiti 0 veya daha büyük olmalıdır.")]
    public decimal? CreditLimit { get; init; }
    
    [StringLength(1000)]
    public string? Terms { get; init; }
}

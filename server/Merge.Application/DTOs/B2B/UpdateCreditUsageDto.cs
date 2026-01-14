using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

/// <summary>
/// Update Credit Usage DTO - BOLUM 4.3: Over-Posting Korumasi (ZORUNLU)
/// Dictionary<string, object> yerine typed DTO kullanılıyor
/// </summary>
public class UpdateCreditUsageDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
    public decimal Amount { get; set; }
}

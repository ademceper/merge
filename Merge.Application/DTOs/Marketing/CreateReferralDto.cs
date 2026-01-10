using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Create Referral DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record CreateReferralDto
{
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Referans kodu en az 3, en fazla 50 karakter olmalıdır.")]
    public string ReferralCode { get; init; } = string.Empty;
}

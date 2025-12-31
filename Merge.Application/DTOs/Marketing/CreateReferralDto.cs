using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class CreateReferralDto
{
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Referans kodu en az 3, en fazla 50 karakter olmalıdır.")]
    public string ReferralCode { get; set; } = string.Empty;
}

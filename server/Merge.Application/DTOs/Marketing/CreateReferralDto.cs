using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Marketing;

namespace Merge.Application.DTOs.Marketing;


public record CreateReferralDto
{
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Referans kodu en az 3, en fazla 50 karakter olmalıdır.")]
    public string ReferralCode { get; init; } = string.Empty;
}

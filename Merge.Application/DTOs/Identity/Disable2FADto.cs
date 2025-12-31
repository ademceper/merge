using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;

namespace Merge.Application.DTOs.Identity;

public class Disable2FADto
{
    [Required]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "2FA kodu en az 4, en fazla 10 karakter olmalıdır.")]
    public string Code { get; set; } = string.Empty; // Current 2FA code to confirm disable
}

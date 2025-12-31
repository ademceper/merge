using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;

namespace Merge.Application.DTOs.Identity;

public class Enable2FADto
{
    [Required]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "Doğrulama kodu en az 4, en fazla 10 karakter olmalıdır.")]
    public string Code { get; set; } = string.Empty; // Verification code
}

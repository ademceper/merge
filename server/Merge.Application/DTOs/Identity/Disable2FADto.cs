using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Identity;

public record Disable2FADto(
    [Required] [StringLength(10, MinimumLength = 4, ErrorMessage = "2FA kodu en az 4, en fazla 10 karakter olmalıdır.")] string Code);

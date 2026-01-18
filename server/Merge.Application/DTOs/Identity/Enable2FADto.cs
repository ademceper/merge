using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Identity;

public record Enable2FADto(
    [Required] [StringLength(10, MinimumLength = 4, ErrorMessage = "Doğrulama kodu en az 4, en fazla 10 karakter olmalıdır.")] string Code);

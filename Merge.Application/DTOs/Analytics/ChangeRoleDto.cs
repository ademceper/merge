using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

public class ChangeRoleDto
{
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Rol adı en az 2, en fazla 50 karakter olmalıdır.")]
    public string Role { get; set; } = string.Empty;
}

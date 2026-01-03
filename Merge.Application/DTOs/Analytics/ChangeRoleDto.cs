using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Change Role DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class ChangeRoleDto
{
    [Required(ErrorMessage = "Rol adı zorunludur")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Rol adı en az 2, en fazla 50 karakter olmalıdır.")]
    public string Role { get; set; } = string.Empty;
}

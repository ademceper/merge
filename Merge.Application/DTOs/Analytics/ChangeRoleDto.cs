using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Change Role DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
/// </summary>
public record ChangeRoleDto(
    [Required(ErrorMessage = "Rol adı zorunludur")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Rol adı en az 2, en fazla 50 karakter olmalıdır.")]
    string Role
);

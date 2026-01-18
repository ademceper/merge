using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.DTOs.Analytics;


public record ChangeRoleDto(
    [Required(ErrorMessage = "Rol adı zorunludur")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Rol adı en az 2, en fazla 50 karakter olmalıdır.")]
    string Role
);

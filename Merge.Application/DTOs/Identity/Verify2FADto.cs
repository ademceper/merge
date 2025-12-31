using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;

namespace Merge.Application.DTOs.Identity;

public class Verify2FADto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "Kod en az 4, en fazla 10 karakter olmalıdır.")]
    public string Code { get; set; } = string.Empty;
}

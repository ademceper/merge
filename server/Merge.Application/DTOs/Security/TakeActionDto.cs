using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class TakeActionDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Aksiyon en az 2, en fazla 100 karakter olmalıdır.")]
    public string Action { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string? Notes { get; set; }
}

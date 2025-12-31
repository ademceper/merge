using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class BlockPaymentDto
{
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Engelleme nedeni en az 5, en fazla 1000 karakter olmalıdır.")]
    public string Reason { get; set; } = string.Empty;
}

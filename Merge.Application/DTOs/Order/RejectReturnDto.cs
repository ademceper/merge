using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Order;

public class RejectReturnDto
{
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Red nedeni en az 5, en fazla 1000 karakter olmalıdır.")]
    public string Reason { get; set; } = string.Empty;
}

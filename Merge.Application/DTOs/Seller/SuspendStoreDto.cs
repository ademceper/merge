using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

public class SuspendStoreDto
{
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Askıya alma nedeni en az 5, en fazla 1000 karakter olmalıdır.")]
    public string Reason { get; set; } = string.Empty;
}

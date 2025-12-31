using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

public class ProcessPayoutDto
{
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "İşlem referansı gereklidir.")]
    public string TransactionReference { get; set; } = string.Empty;
}

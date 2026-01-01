using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public class ConvertPriceDto
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 999999999.99, ErrorMessage = "Amount must be a positive value")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Source currency code is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
    public string FromCurrency { get; set; } = string.Empty;

    [Required(ErrorMessage = "Target currency code is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
    public string ToCurrency { get; set; } = string.Empty;
}

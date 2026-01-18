using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.International;

public record ConvertPriceDto(
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 999999999.99, ErrorMessage = "Amount must be a positive value")]
    decimal Amount,

    [Required(ErrorMessage = "Source currency code is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
    string FromCurrency,

    [Required(ErrorMessage = "Target currency code is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
    string ToCurrency);

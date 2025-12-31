using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public class CreateStaticTranslationDto
{
    [Required]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Anahtar gereklidir ve en fazla 200 karakter olmalıdır.")]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Dil kodu en az 2, en fazla 10 karakter olmalıdır.")]
    public string LanguageCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Değer gereklidir ve en fazla 5000 karakter olmalıdır.")]
    public string Value { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;
}

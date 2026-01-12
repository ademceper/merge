using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record CreateStaticTranslationDto(
    [Required]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Anahtar gereklidir ve en fazla 200 karakter olmalıdır.")]
    string Key,
    
    [Required]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Dil kodu en az 2, en fazla 10 karakter olmalıdır.")]
    string LanguageCode,
    
    [Required]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Değer gereklidir ve en fazla 5000 karakter olmalıdır.")]
    string Value,
    
    [Required]
    [StringLength(100)]
    string Category);

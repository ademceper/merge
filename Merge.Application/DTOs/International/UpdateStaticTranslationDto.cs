using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record UpdateStaticTranslationDto(
    [Required]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Değer gereklidir ve en fazla 5000 karakter olmalıdır.")]
    string Value,
    
    [Required]
    [StringLength(100)]
    string Category);


using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record BulkTranslationDto(
    [Required]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Dil kodu en az 2, en fazla 10 karakter olmalıdır.")]
    string LanguageCode,
    
    [Required]
    [MinLength(1, ErrorMessage = "En az bir çeviri gereklidir.")]
    Dictionary<string, string> Translations);

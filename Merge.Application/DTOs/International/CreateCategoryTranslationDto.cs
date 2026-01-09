using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record CreateCategoryTranslationDto(
    [Required]
    Guid CategoryId,
    
    [Required]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Dil kodu en az 2, en fazla 10 karakter olmalıdır.")]
    string LanguageCode,
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 200 karakter olmalıdır.")]
    string Name,
    
    [StringLength(2000)]
    string Description = "");

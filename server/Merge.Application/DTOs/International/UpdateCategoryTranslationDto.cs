using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record UpdateCategoryTranslationDto(
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 200 karakter olmalıdır.")]
    string Name,
    
    [StringLength(2000)]
    string Description = "");


using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.LiveCommerce;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record AddProductToStreamDto(
    [Range(0, int.MaxValue)] int DisplayOrder,
    bool IsHighlighted,
    [Range(0, double.MaxValue, ErrorMessage = "Özel fiyat 0 veya daha büyük olmalıdır.")]
    decimal? SpecialPrice,
    [StringLength(500)] string? ShowcaseNotes);

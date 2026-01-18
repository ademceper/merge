using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.LiveCommerce;

// NOT: IsHighlighted field'ı kaldırıldı çünkü showcase işlemi ayrı bir command (ShowcaseProductCommand)
public record AddProductToStreamDto(
    [Range(0, int.MaxValue)] int DisplayOrder,
    [Range(0, double.MaxValue, ErrorMessage = "Özel fiyat 0 veya daha büyük olmalıdır.")]
    decimal? SpecialPrice,
    [StringLength(1000)] string? ShowcaseNotes);

using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Catalog;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
// ✅ BOLUM 4.1: Validation Attributes (ZORUNLU)
public record CategoryDto(
    Guid Id,
    [Required(ErrorMessage = "Kategori adı zorunludur")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 200 karakter olmalıdır")]
    string Name,
    [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
    string Description,
    [Required(ErrorMessage = "Slug zorunludur")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Slug en az 2, en fazla 200 karakter olmalıdır")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug sadece küçük harf, rakam ve tire içerebilir")]
    string Slug,
    [StringLength(500, ErrorMessage = "Resim URL'si en fazla 500 karakter olabilir")]
    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    string ImageUrl,
    Guid? ParentCategoryId,
    // Output-only properties (mapping'den gelir)
    string? ParentCategoryName = null,
    IReadOnlyList<CategoryDto>? SubCategories = null
);

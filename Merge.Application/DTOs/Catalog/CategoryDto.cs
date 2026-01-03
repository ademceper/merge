using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Catalog;

// ✅ BOLUM 4.1: Validation Attributes (ZORUNLU)
public class CategoryDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Kategori adı zorunludur")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 200 karakter olmalıdır")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Slug zorunludur")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Slug en az 2, en fazla 200 karakter olmalıdır")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug sadece küçük harf, rakam ve tire içerebilir")]
    public string Slug { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Resim URL'si en fazla 500 karakter olabilir")]
    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    public string ImageUrl { get; set; } = string.Empty;
    
    public Guid? ParentCategoryId { get; set; }
    
    // Output-only properties (mapping'den gelir)
    public string? ParentCategoryName { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
}

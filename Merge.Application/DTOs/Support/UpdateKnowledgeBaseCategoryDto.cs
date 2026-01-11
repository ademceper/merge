using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

public class UpdateKnowledgeBaseCategoryDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string? Name { get; set; }
    
    // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - DTO validation matches SupportSettings.MaxCategoryDescriptionLength=1000
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public Guid? ParentCategoryId { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? DisplayOrder { get; set; }
    
    public bool? IsActive { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? IconUrl { get; set; }
}

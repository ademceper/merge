using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

public class CreateKnowledgeBaseCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public Guid? ParentCategoryId { get; set; }
    
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? IconUrl { get; set; }
}

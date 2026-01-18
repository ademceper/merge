using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;


public record UpdateKnowledgeBaseCategoryDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string? Name { get; init; }
    
    [StringLength(1000)]
    public string? Description { get; init; }
    
    public Guid? ParentCategoryId { get; init; }
    
    [Range(0, int.MaxValue)]
    public int? DisplayOrder { get; init; }
    
    public bool? IsActive { get; init; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? IconUrl { get; init; }
}

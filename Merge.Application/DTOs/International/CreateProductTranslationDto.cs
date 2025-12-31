using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public class CreateProductTranslationDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Dil kodu en az 2, en fazla 10 karakter olmalıdır.")]
    public string LanguageCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Ürün adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(5000)]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string ShortDescription { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string MetaTitle { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string MetaDescription { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string MetaKeywords { get; set; } = string.Empty;
}

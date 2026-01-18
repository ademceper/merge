using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public record CreateProductTranslationDto(
    [Required]
    Guid ProductId,
    
    [Required]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Dil kodu en az 2, en fazla 10 karakter olmalıdır.")]
    string LanguageCode,
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Ürün adı en az 2, en fazla 200 karakter olmalıdır.")]
    string Name,
    
    [StringLength(5000)]
    string Description = "",
    
    [StringLength(500)]
    string ShortDescription = "",
    
    [StringLength(200)]
    string MetaTitle = "",
    
    [StringLength(500)]
    string MetaDescription = "",
    
    [StringLength(200)]
    string MetaKeywords = "");

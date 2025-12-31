using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public class CreateCategoryTranslationDto
{
    [Required]
    public Guid CategoryId { get; set; }
    
    [Required]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Dil kodu en az 2, en fazla 10 karakter olmalıdır.")]
    public string LanguageCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
}

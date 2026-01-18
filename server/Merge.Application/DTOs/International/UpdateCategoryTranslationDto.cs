using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.International;

public record UpdateCategoryTranslationDto(
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kategori adı en az 2, en fazla 200 karakter olmalıdır.")]
    string Name,
    
    [StringLength(2000)]
    string Description = "");


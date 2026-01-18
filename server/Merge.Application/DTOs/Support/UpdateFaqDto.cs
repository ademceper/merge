using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Support;


public record UpdateFaqDto
{
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Soru en az 5, en fazla 500 karakter olmalıdır.")]
    public string Question { get; init; } = string.Empty;
    
    [Required]
    [StringLength(5000, MinimumLength = 5, ErrorMessage = "Cevap en az 5, en fazla 5000 karakter olmalıdır.")]
    public string Answer { get; init; } = string.Empty;
    
    [StringLength(50)]
    public string Category { get; init; } = "General";
    
    [Range(0, int.MaxValue)]
    public int SortOrder { get; init; } = 0;
    
    public bool IsPublished { get; init; } = true;
}

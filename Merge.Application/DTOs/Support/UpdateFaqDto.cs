using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

public class UpdateFaqDto
{
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Soru en az 5, en fazla 500 karakter olmalıdır.")]
    public string Question { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Cevap en az 5, en fazla 2000 karakter olmalıdır.")]
    public string Answer { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Category { get; set; } = "General";
    
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; } = 0;
    
    public bool IsPublished { get; set; } = true;
}

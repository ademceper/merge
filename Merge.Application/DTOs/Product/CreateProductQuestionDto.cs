using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class CreateProductQuestionDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Soru en az 5, en fazla 1000 karakter olmalıdır.")]
    public string Question { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class CreateProductAnswerDto
{
    [Required]
    public Guid QuestionId { get; set; }
    
    [Required]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Cevap en az 5, en fazla 2000 karakter olmalıdır.")]
    public string Answer { get; set; } = string.Empty;
}

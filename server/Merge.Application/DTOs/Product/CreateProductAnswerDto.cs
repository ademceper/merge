using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

public record CreateProductAnswerDto(
    [Required] Guid QuestionId,
    [Required]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Cevap en az 5, en fazla 2000 karakter olmalıdır.")]
    string Answer
);

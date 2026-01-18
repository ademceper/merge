using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

public record CreateProductQuestionDto(
    [Required] Guid ProductId,
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Soru en az 5, en fazla 1000 karakter olmalıdır.")]
    string Question
);

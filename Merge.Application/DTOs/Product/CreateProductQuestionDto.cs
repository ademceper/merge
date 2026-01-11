using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record CreateProductQuestionDto(
    [Required] Guid ProductId,
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Soru en az 5, en fazla 1000 karakter olmalıdır.")]
    string Question
);

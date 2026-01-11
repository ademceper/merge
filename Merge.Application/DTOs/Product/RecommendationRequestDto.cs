using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record RecommendationRequestDto(
    Guid? ProductId,
    Guid? CategoryId,
    [Range(1, 100, ErrorMessage = "Maksimum sonuç sayısı 1 ile 100 arasında olmalıdır.")]
    int MaxResults = 10
);

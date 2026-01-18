using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

public record RecommendationRequestDto(
    Guid? ProductId,
    Guid? CategoryId,
    [Range(1, 100, ErrorMessage = "Maksimum sonuç sayısı 1 ile 100 arasında olmalıdır.")]
    int MaxResults = 10
);

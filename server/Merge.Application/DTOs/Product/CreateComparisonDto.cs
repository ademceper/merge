using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

public record CreateComparisonDto(
    [StringLength(200)] string? Name,
    [Required]
    [MinLength(2, ErrorMessage = "En az 2 ürün karşılaştırılmalıdır.")]
    IReadOnlyList<Guid> ProductIds
);

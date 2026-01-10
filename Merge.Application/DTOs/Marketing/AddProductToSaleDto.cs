using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Add Product To Sale DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record AddProductToSaleDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; init; }

    [Required(ErrorMessage = "Sale price is required")]
    [Range(0.01, 999999999.99, ErrorMessage = "Sale price must be a positive value")]
    public decimal SalePrice { get; init; }

    [Required(ErrorMessage = "Stock limit is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Stock limit must be at least 1")]
    public int StockLimit { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a non-negative value")]
    public int SortOrder { get; init; } = 0;
}

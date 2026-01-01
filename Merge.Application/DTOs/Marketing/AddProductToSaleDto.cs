using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class AddProductToSaleDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Sale price is required")]
    [Range(0.01, 999999999.99, ErrorMessage = "Sale price must be a positive value")]
    public decimal SalePrice { get; set; }

    [Required(ErrorMessage = "Stock limit is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Stock limit must be at least 1")]
    public int StockLimit { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a non-negative value")]
    public int SortOrder { get; set; } = 0;
}

using Merge.Domain.Modules.Identity;
namespace Merge.Application.DTOs.User;

public class PopularProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int AddToCartCount { get; set; }
    public int PurchaseCount { get; set; }
    public decimal ConversionRate { get; set; }
}

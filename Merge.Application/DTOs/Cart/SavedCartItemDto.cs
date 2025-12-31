namespace Merge.Application.DTOs.Cart;

public class SavedCartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal CurrentPrice { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public bool IsPriceChanged { get; set; }
}

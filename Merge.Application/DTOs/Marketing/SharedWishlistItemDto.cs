namespace Merge.Application.DTOs.Marketing;

public class SharedWishlistItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Note { get; set; } = string.Empty;
    public bool IsPurchased { get; set; }
}

namespace Merge.Application.DTOs.Cart;

public class SaveItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}

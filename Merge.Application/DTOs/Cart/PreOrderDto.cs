namespace Merge.Application.DTOs.Cart;

public class PreOrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal DepositPaid { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExpectedAvailabilityDate { get; set; }
    public DateTime? ActualAvailabilityDate { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

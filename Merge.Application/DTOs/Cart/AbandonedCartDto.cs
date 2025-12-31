namespace Merge.Application.DTOs.Cart;

public class AbandonedCartDto
{
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime LastModified { get; set; }
    public int HoursSinceAbandonment { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public bool HasReceivedEmail { get; set; }
    public int EmailsSentCount { get; set; }
    public DateTime? LastEmailSent { get; set; }
}

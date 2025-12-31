namespace Merge.Application.DTOs.Cart;

public class PayPreOrderDepositDto
{
    public Guid PreOrderId { get; set; }
    public decimal Amount { get; set; }
}

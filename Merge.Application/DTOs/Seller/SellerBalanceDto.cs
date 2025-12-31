namespace Merge.Application.DTOs.Seller;

public class SellerBalanceDto
{
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public decimal TotalEarnings { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal InTransitBalance { get; set; } // Payouts being processed
    public decimal TotalPayouts { get; set; }
    public decimal NextPayoutDate { get; set; } // Days until next payout
}

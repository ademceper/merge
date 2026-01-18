namespace Merge.Application.DTOs.Seller;

public record SellerBalanceDto
{
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public decimal TotalEarnings { get; init; }
    public decimal PendingBalance { get; init; }
    public decimal AvailableBalance { get; init; }
    public decimal InTransitBalance { get; init; } // Payouts being processed
    public decimal TotalPayouts { get; init; }
    public decimal NextPayoutDate { get; init; } // Days until next payout
}

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
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

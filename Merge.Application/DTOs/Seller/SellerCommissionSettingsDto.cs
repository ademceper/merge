namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record SellerCommissionSettingsDto
{
    public Guid SellerId { get; init; }
    public decimal CustomCommissionRate { get; init; }
    public bool UseCustomRate { get; init; }
    public decimal MinimumPayoutAmount { get; init; }
    public string? PaymentMethod { get; init; }
    public string? PaymentDetails { get; init; }
}

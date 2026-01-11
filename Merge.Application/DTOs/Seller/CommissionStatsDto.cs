namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record CommissionStatsDto
{
    public int TotalCommissions { get; init; }
    public decimal TotalEarnings { get; init; }
    public decimal PendingCommissions { get; init; }
    public decimal ApprovedCommissions { get; init; }
    public decimal PaidCommissions { get; init; }
    public decimal AvailableForPayout { get; init; }
    public decimal AverageCommissionRate { get; init; }
    public decimal TotalPlatformFees { get; init; }
    public Dictionary<string, decimal> CommissionsByMonth { get; init; } = new();
}

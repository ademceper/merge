namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record CommissionTierDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal MinSales { get; init; }
    public decimal MaxSales { get; init; }
    public decimal CommissionRate { get; init; }
    public decimal PlatformFeeRate { get; init; }
    public bool IsActive { get; init; }
    public int Priority { get; init; }
}

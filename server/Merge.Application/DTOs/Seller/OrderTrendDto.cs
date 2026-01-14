namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record OrderTrendDto
{
    public DateTime Date { get; init; }
    public int OrderCount { get; init; }
    public int CompletedOrders { get; init; }
    public int CancelledOrders { get; init; }
}

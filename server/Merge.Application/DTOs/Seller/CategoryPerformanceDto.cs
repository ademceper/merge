namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record CategoryPerformanceDto
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int ProductCount { get; init; }
    public int OrderCount { get; init; }
    public int OrdersCount { get; init; } // Alias for OrderCount
    public decimal TotalSales { get; init; }
    public decimal Revenue { get; init; }
    public decimal AverageRating { get; init; }
}

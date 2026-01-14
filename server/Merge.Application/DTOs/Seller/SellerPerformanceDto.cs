namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record SellerPerformanceDto
{
    public decimal TotalSales { get; init; }
    public int TotalOrders { get; init; }
    public decimal AverageOrderValue { get; init; }
    public decimal ConversionRate { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalCustomers { get; init; }
    public List<SalesByDateDto> SalesByDate { get; init; } = new();
    public List<SellerTopProductDto> TopProducts { get; init; } = new();
}

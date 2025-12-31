namespace Merge.Application.DTOs.Seller;

public class SellerPerformanceDto
{
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalCustomers { get; set; }
    public List<SalesByDateDto> SalesByDate { get; set; } = new List<SalesByDateDto>();
    public List<SellerTopProductDto> TopProducts { get; set; } = new List<SellerTopProductDto>();
}

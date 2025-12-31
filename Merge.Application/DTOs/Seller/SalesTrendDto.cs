namespace Merge.Application.DTOs.Seller;

public class SalesTrendDto
{
    public DateTime Date { get; set; }
    public decimal Sales { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

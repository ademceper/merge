namespace Merge.Application.DTOs.Seller;

public record SalesTrendDto
{
    public DateTime Date { get; init; }
    public decimal Sales { get; init; }
    public int OrderCount { get; init; }
    public decimal AverageOrderValue { get; init; }
}

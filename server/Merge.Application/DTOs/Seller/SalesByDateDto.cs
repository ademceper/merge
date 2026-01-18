namespace Merge.Application.DTOs.Seller;

public record SalesByDateDto
{
    public DateTime Date { get; init; }
    public decimal Sales { get; init; }
    public int OrderCount { get; init; }
}

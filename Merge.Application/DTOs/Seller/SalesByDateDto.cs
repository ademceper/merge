namespace Merge.Application.DTOs.Seller;

public class SalesByDateDto
{
    public DateTime Date { get; set; }
    public decimal Sales { get; set; }
    public int OrderCount { get; set; }
}

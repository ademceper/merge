namespace Merge.Application.DTOs.Marketing;

public class AddProductToSaleDto
{
    public Guid ProductId { get; set; }
    public decimal SalePrice { get; set; }
    public int StockLimit { get; set; }
    public int SortOrder { get; set; } = 0;
}

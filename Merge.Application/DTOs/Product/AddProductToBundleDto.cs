namespace Merge.Application.DTOs.Product;

public class AddProductToBundleDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
}

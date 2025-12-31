namespace Merge.Application.DTOs.Product;

public class ComparisonMatrixDto
{
    public List<string> AttributeNames { get; set; } = new();
    public List<ComparisonProductDto> Products { get; set; } = new();
    public Dictionary<string, List<string>> AttributeValues { get; set; } = new(); // Key: attribute name, Value: list of values for each product
}

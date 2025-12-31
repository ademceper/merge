namespace Merge.Application.DTOs.Product;

public class BulkProductExportDto
{
    public Guid? CategoryId { get; set; }
    public bool ActiveOnly { get; set; } = true;
    public bool IncludeVariants { get; set; } = false;
}

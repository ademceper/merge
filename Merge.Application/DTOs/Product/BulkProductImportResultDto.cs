namespace Merge.Application.DTOs.Product;

public class BulkProductImportResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<ProductDto> ImportedProducts { get; set; } = new();
}

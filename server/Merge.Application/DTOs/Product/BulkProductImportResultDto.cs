using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record BulkProductImportResultDto(
    int TotalProcessed,
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<string> Errors,
    IReadOnlyList<ProductDto> ImportedProducts
);

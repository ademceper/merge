using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record BulkProductImportResultDto(
    int TotalProcessed,
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<string> Errors,
    IReadOnlyList<ProductDto> ImportedProducts
);

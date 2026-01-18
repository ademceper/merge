using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record BulkProductExportDto(
    Guid? CategoryId,
    bool ActiveOnly = true,
    bool IncludeVariants = false
);

using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Product;

public record BulkProductImportDto(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    decimal? DiscountPrice,
    int StockQuantity,
    string Brand,
    string ImageUrl,
    string CategoryName, // Will be matched to existing category
    bool IsActive = true
);

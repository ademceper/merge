using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Product;

public record ComparisonProductDto(
    Guid ProductId,
    string Name,
    string SKU,
    decimal Price,
    decimal? DiscountPrice,
    string? MainImage,
    string Brand,
    string Category,
    int StockQuantity,
    decimal? Rating,
    int ReviewCount,
    IReadOnlyDictionary<string, string> Specifications,
    IReadOnlyList<string> Features,
    int Position
);

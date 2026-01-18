using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Product;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string SKU,
    decimal Price,
    decimal? DiscountPrice,
    int StockQuantity,
    string Brand,
    string ImageUrl,
    IReadOnlyList<string> ImageUrls,
    decimal Rating,
    int ReviewCount,
    bool IsActive,
    Guid CategoryId,
    string CategoryName,
    Guid? SellerId,
    Guid? StoreId
);


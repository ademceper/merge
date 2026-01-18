using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Marketing;


public record FlashSaleProductDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    decimal OriginalPrice,
    decimal SalePrice,
    int StockLimit,
    int SoldQuantity,
    int AvailableQuantity,
    decimal DiscountPercentage,
    int SortOrder);

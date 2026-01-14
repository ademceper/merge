using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Flash Sale Product DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
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

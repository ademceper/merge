namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
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
    // ✅ SECURITY: Seller ownership için gerekli
    Guid? SellerId,
    Guid? StoreId
);


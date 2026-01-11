namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
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

using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record BundleItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    decimal ProductPrice,
    int Quantity,
    int SortOrder
);

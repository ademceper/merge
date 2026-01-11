namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record AddProductToBundleDto(
    Guid ProductId,
    int Quantity = 1,
    int SortOrder = 0
);

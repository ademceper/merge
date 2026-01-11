namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record ComparisonMatrixDto(
    IReadOnlyList<string> AttributeNames,
    IReadOnlyList<ComparisonProductDto> Products,
    IReadOnlyDictionary<string, IReadOnlyList<string>> AttributeValues // Key: attribute name, Value: list of values for each product
);

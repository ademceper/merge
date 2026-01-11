namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record ProductComparisonDto(
    Guid Id,
    Guid UserId, // ✅ SECURITY: IDOR koruması için gerekli
    string Name,
    bool IsSaved,
    string? ShareCode,
    IReadOnlyList<ComparisonProductDto> Products,
    DateTime CreatedAt
);

using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record ProductComparisonDto(
    Guid Id,
    Guid UserId, // ✅ SECURITY: IDOR koruması için gerekli
    string Name,
    bool IsSaved,
    string? ShareCode,
    IReadOnlyList<ComparisonProductDto> Products,
    DateTime CreatedAt
);

using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record ProductTemplateDto(
    Guid Id,
    string Name,
    string Description,
    Guid CategoryId,
    string CategoryName,
    string? Brand,
    string? DefaultSKUPrefix,
    decimal? DefaultPrice,
    int? DefaultStockQuantity,
    string? DefaultImageUrl,
    IReadOnlyDictionary<string, string>? Specifications,
    IReadOnlyDictionary<string, string>? Attributes,
    bool IsActive,
    int UsageCount,
    DateTime CreatedAt
);

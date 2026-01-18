using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record ProductSizeGuideDto(
    Guid ProductId,
    string ProductName,
    SizeGuideDto SizeGuide,
    string? CustomNotes,
    string? FitDescription
);

using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record ComparisonMatrixDto(
    IReadOnlyList<string> AttributeNames,
    IReadOnlyList<ComparisonProductDto> Products,
    IReadOnlyDictionary<string, IReadOnlyList<string>> AttributeValues // Key: attribute name, Value: list of values for each product
);

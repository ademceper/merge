using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Search;

public record SearchSuggestionDto(
    string Term,
    string Type, // Product, Category, Brand
    int Frequency,
    Guid? ReferenceId = null
);

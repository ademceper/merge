namespace Merge.Application.DTOs.Search;

public record ProductSuggestionDto(
    Guid Id,
    string Name,
    string CategoryName,
    decimal Price,
    string ImageUrl
);

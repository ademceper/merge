namespace Merge.Application.DTOs.Search;

public record SearchRequestDto(
    string? SearchTerm,
    Guid? CategoryId,
    string? Brand,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinRating,
    bool InStockOnly,
    string? SortBy, // price_asc, price_desc, rating, newest, popular
    int? Page,
    int? PageSize
);

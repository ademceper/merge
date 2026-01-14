namespace Merge.Application.DTOs.Search;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
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

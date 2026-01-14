namespace Merge.Application.DTOs.Search;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record ProductSuggestionDto(
    Guid Id,
    string Name,
    string CategoryName,
    decimal Price,
    string ImageUrl
);

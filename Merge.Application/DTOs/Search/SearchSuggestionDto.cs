namespace Merge.Application.DTOs.Search;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record SearchSuggestionDto(
    string Term,
    string Type, // Product, Category, Brand
    int Frequency,
    Guid? ReferenceId = null
);

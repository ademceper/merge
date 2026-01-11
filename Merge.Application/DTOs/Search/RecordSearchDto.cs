namespace Merge.Application.DTOs.Search;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record RecordSearchDto(
    string SearchTerm,
    int ResultCount
);

namespace Merge.Application.DTOs.Search;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record RecordClickDto(
    Guid SearchHistoryId,
    Guid ProductId
);

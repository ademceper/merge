namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record PickPackItemStatusDto(
    bool IsPicked,
    bool IsPacked,
    string? Location = null
);

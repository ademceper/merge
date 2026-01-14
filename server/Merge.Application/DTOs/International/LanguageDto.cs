namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record LanguageDto(
    Guid Id,
    string Code,
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive,
    bool IsRTL,
    string FlagIcon);

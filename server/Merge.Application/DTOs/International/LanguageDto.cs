namespace Merge.Application.DTOs.International;

public record LanguageDto(
    Guid Id,
    string Code,
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive,
    bool IsRTL,
    string FlagIcon);

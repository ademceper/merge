namespace Merge.Application.DTOs.Governance;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record PolicyDto(
    Guid Id,
    string PolicyType,
    string Title,
    string Content,
    string Version,
    bool IsActive,
    bool RequiresAcceptance,
    DateTime? EffectiveDate,
    DateTime? ExpiryDate,
    Guid? CreatedByUserId,
    string? CreatedByName,
    string? ChangeLog,
    string Language,
    int AcceptanceCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Identity;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record TwoFactorStatusDto(
    bool IsEnabled,
    TwoFactorMethod Method,
    string? PhoneNumber,
    string? Email,
    int BackupCodesRemaining);

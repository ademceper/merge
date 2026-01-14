namespace Merge.Application.DTOs.Identity;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record TwoFactorSetupResponseDto(
    string Secret,
    string QrCodeUrl,
    string[] BackupCodes,
    string Message);

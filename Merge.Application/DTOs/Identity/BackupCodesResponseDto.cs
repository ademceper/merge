namespace Merge.Application.DTOs.Identity;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record BackupCodesResponseDto(
    string[] BackupCodes,
    string Message);

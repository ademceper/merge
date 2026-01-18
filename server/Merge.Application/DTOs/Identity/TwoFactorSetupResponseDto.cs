namespace Merge.Application.DTOs.Identity;

public record TwoFactorSetupResponseDto(
    string Secret,
    string QrCodeUrl,
    string[] BackupCodes,
    string Message);

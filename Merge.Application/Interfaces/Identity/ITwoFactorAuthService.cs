using Merge.Application.DTOs.Identity;
namespace Merge.Application.Interfaces.Identity;

public interface ITwoFactorAuthService
{
    Task<TwoFactorSetupResponseDto> Setup2FAAsync(Guid userId, TwoFactorSetupDto setupDto);
    Task<bool> Enable2FAAsync(Guid userId, Enable2FADto enableDto);
    Task<bool> Disable2FAAsync(Guid userId, Disable2FADto disableDto);
    Task<TwoFactorStatusDto?> Get2FAStatusAsync(Guid userId);
    Task<bool> Verify2FACodeAsync(Guid userId, string code);
    Task<bool> SendVerificationCodeAsync(Guid userId, string purpose = "Login");
    Task<BackupCodesResponseDto> RegenerateBackupCodesAsync(Guid userId, RegenerateBackupCodesDto regenerateDto);
    Task<bool> VerifyBackupCodeAsync(Guid userId, string backupCode);
    string GenerateTOTPSecret();
    string GenerateQRCodeUrl(string secret, string email);
}

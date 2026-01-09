using Merge.Application.DTOs.Identity;
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Identity;

// ⚠️ OBSOLETE: Bu interface artık kullanılmamalı. MediatR Command/Query pattern kullanılmalı.
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service'ler yerine Command/Query handler'ları kullan
[Obsolete("Use MediatR Commands/Queries instead. This interface will be removed in a future version.")]
public interface ITwoFactorAuthService
{
    Task<TwoFactorSetupResponseDto> Setup2FAAsync(Guid userId, TwoFactorSetupDto setupDto, CancellationToken cancellationToken = default);
    Task<bool> Enable2FAAsync(Guid userId, Enable2FADto enableDto, CancellationToken cancellationToken = default);
    Task<bool> Disable2FAAsync(Guid userId, Disable2FADto disableDto, CancellationToken cancellationToken = default);
    Task<TwoFactorStatusDto?> Get2FAStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> Verify2FACodeAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task<bool> SendVerificationCodeAsync(Guid userId, string purpose = "Login", CancellationToken cancellationToken = default);
    Task<BackupCodesResponseDto> RegenerateBackupCodesAsync(Guid userId, RegenerateBackupCodesDto regenerateDto, CancellationToken cancellationToken = default);
    Task<bool> VerifyBackupCodeAsync(Guid userId, string backupCode, CancellationToken cancellationToken = default);
    string GenerateTOTPSecret();
    string GenerateQRCodeUrl(string secret, string email);
}

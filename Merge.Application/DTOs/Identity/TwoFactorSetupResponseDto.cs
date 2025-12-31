using Merge.Domain.Entities;
namespace Merge.Application.DTOs.Identity;

public class TwoFactorSetupResponseDto
{
    public string Secret { get; set; } = string.Empty; // For authenticator apps
    public string QrCodeUrl { get; set; } = string.Empty; // QR code data URL for authenticator apps
    public string[] BackupCodes { get; set; } = Array.Empty<string>();
    public string Message { get; set; } = string.Empty;
}

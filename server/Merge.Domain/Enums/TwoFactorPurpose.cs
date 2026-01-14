namespace Merge.Domain.Enums;

/// <summary>
/// Two Factor Purpose Enum - BOLUM 1.2: Enum kullanımı (string Purpose YASAK)
/// </summary>
public enum TwoFactorPurpose
{
    Login = 0,
    Enable2FA = 1,
    Disable2FA = 2,
    ChangePassword = 3,
    ChangeEmail = 4,
    WithdrawFunds = 5
}

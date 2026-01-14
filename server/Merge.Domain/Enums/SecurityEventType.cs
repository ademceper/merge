namespace Merge.Domain.Enums;

/// <summary>
/// Security Event Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum SecurityEventType
{
    Login = 0,
    Logout = 1,
    PasswordChange = 2,
    EmailChange = 3,
    SuspiciousActivity = 4,
    FailedLogin = 5,
    AccountLocked = 6,
    AccountUnlocked = 7,
    PasswordResetRequested = 8,
    PasswordResetCompleted = 9,
    TwoFactorEnabled = 10,
    TwoFactorDisabled = 11,
    RoleChanged = 12,
    PermissionChanged = 13,
    UnauthorizedAccess = 14,
    DataExport = 15,
    DataImport = 16,
    BulkOperation = 17
}

namespace Merge.Domain.Enums;

/// <summary>
/// Audit Action - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum AuditAction
{
    // Authentication & Authorization
    Login,
    Logout,
    LoginFailed,
    Register,
    PasswordChanged,
    PasswordResetRequested,
    PasswordResetCompleted,
    TwoFactorEnabled,
    TwoFactorDisabled,
    RoleChanged,
    PermissionChanged,

    // CRUD Operations
    Created,
    Updated,
    Deleted,
    SoftDeleted,
    Restored,

    // Product Management
    ProductCreated,
    ProductUpdated,
    ProductDeleted,
    ProductPriceChanged,
    ProductStockChanged,
    ProductPublished,
    ProductUnpublished,

    // Order Management
    OrderCreated,
    OrderUpdated,
    OrderCanceled,
    OrderRefunded,
    OrderStatusChanged,

    // User Management
    UserCreated,
    UserUpdated,
    UserDeleted,
    UserActivated,
    UserDeactivated,
    UserRoleChanged,

    // Payment Operations
    PaymentProcessed,
    PaymentFailed,
    RefundIssued,

    // System Configuration
    SettingsChanged,
    ConfigurationUpdated,

    // Security Events
    UnauthorizedAccess,
    SuspiciousActivity,
    DataExport,
    DataImport,
    BulkOperation
}


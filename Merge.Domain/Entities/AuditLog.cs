namespace Merge.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; } // User who performed the action
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, etc.
    public string EntityType { get; set; } = string.Empty; // Product, Order, User, etc.
    public Guid? EntityId { get; set; } // ID of the affected entity
    public string TableName { get; set; } = string.Empty; // Database table name
    public string PrimaryKey { get; set; } = string.Empty; // Primary key value
    public string OldValues { get; set; } = string.Empty; // JSON of old values
    public string NewValues { get; set; } = string.Empty; // JSON of new values
    public string Changes { get; set; } = string.Empty; // Summary of changes
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public AuditSeverity Severity { get; set; } = AuditSeverity.Info;
    public string Module { get; set; } = string.Empty; // Auth, Products, Orders, etc.
    public bool IsSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string AdditionalData { get; set; } = string.Empty; // JSON for extra context

    // Navigation properties
    public User? User { get; set; }
}

public enum AuditSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}

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

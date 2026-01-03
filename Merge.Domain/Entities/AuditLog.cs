using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// AuditLog Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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


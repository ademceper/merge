using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.SharedKernel;

/// <summary>
/// AuditLog Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class AuditLog : BaseEntity, IAggregateRoot
{
    public Guid? UserId { get; private set; } // User who performed the action
    public string UserEmail { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty; // Create, Update, Delete, Login, etc.
    public string EntityType { get; private set; } = string.Empty; // Product, Order, User, etc.
    public Guid? EntityId { get; private set; } // ID of the affected entity
    public string TableName { get; private set; } = string.Empty; // Database table name
    public string PrimaryKey { get; private set; } = string.Empty; // Primary key value
    public string OldValues { get; private set; } = string.Empty; // JSON of old values
    public string NewValues { get; private set; } = string.Empty; // JSON of new values
    public string Changes { get; private set; } = string.Empty; // Summary of changes
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public AuditSeverity Severity { get; private set; } = AuditSeverity.Info;
    public string Module { get; private set; } = string.Empty; // Auth, Products, Orders, etc.
    public bool IsSuccessful { get; private set; } = true;
    public string? ErrorMessage { get; private set; }
    public string AdditionalData { get; private set; } = string.Empty; // JSON for extra context

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User? User { get; set; }

    private AuditLog() { }

    public static AuditLog Create(
        string action,
        string entityType,
        string tableName,
        string ipAddress,
        string userAgent,
        string module,
        AuditSeverity severity = AuditSeverity.Info,
        Guid? userId = null,
        string? userEmail = null,
        Guid? entityId = null,
        string? primaryKey = null,
        string? oldValues = null,
        string? newValues = null,
        string? changes = null,
        string? additionalData = null,
        bool isSuccessful = true,
        string? errorMessage = null)
    {
        Guard.AgainstNullOrEmpty(action, nameof(action));
        Guard.AgainstNullOrEmpty(entityType, nameof(entityType));
        Guard.AgainstNullOrEmpty(tableName, nameof(tableName));
        Guard.AgainstNullOrEmpty(ipAddress, nameof(ipAddress));
        Guard.AgainstNullOrEmpty(userAgent, nameof(userAgent));
        Guard.AgainstNullOrEmpty(module, nameof(module));

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = userEmail ?? string.Empty,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            TableName = tableName,
            PrimaryKey = primaryKey ?? string.Empty,
            OldValues = oldValues ?? string.Empty,
            NewValues = newValues ?? string.Empty,
            Changes = changes ?? string.Empty,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Severity = severity,
            Module = module,
            AdditionalData = additionalData ?? string.Empty,
            IsSuccessful = isSuccessful,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow
        };

        log.AddDomainEvent(new AuditLogCreatedEvent(
            log.Id,
            action,
            entityType,
            userId,
            userEmail ?? string.Empty,
            severity,
            module,
            isSuccessful));

        return log;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));
        IsSuccessful = false;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSeverity(AuditSeverity severity)
    {
        Severity = severity;
        UpdatedAt = DateTime.UtcNow;
    }
}


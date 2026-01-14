using Merge.Domain.Enums;
using Merge.Domain.Entities;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// AuditLog Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AuditLogCreatedEvent(
    Guid AuditLogId,
    string Action,
    string EntityType,
    Guid? UserId,
    string UserEmail,
    AuditSeverity Severity,
    string Module,
    bool IsSuccessful) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


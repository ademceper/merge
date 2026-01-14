using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Policy Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PolicyCreatedEvent(
    Guid PolicyId,
    string PolicyType,
    string Version,
    Guid? CreatedByUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


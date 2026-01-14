using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PolicyAcceptance Restored Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record PolicyAcceptanceRestoredEvent(
    Guid Id,
    Guid PolicyId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PolicyAcceptance Deleted Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record PolicyAcceptanceDeletedEvent(
    Guid AcceptanceId,
    Guid PolicyId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

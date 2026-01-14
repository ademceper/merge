using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Support Ticket Status Changed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SupportTicketStatusChangedEvent(
    Guid TicketId,
    string TicketNumber,
    string OldStatus,
    string NewStatus) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

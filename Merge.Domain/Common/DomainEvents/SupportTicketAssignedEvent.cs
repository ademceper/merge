using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Support Ticket Assigned Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SupportTicketAssignedEvent(
    Guid TicketId,
    string TicketNumber,
    Guid AssignedToId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

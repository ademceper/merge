using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Support Ticket Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SupportTicketCreatedEvent(
    Guid TicketId,
    string TicketNumber,
    Guid UserId,
    string Category,
    string Priority,
    string Subject) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

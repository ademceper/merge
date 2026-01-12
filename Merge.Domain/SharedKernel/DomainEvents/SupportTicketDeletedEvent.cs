using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Support Ticket Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SupportTicketDeletedEvent(
    Guid TicketId,
    string TicketNumber,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

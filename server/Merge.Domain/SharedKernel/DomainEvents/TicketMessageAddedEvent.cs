using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Ticket Message Added Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TicketMessageAddedEvent(
    Guid MessageId,
    Guid TicketId,
    string TicketNumber,
    Guid UserId,
    bool IsStaffResponse) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

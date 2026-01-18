using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TicketMessageAddedEvent(
    Guid MessageId,
    Guid TicketId,
    string TicketNumber,
    Guid UserId,
    bool IsStaffResponse) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

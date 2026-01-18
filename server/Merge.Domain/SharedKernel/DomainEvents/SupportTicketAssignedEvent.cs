using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SupportTicketAssignedEvent(
    Guid TicketId,
    string TicketNumber,
    Guid AssignedToId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

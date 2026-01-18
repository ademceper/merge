using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SupportTicketStatusChangedEvent(
    Guid TicketId,
    string TicketNumber,
    string OldStatus,
    string NewStatus) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SupportTicketClosedEvent(
    Guid TicketId,
    string TicketNumber,
    Guid UserId,
    DateTime ClosedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

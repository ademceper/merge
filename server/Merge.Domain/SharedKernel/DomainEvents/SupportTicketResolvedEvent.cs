using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SupportTicketResolvedEvent(
    Guid TicketId,
    string TicketNumber,
    Guid UserId,
    DateTime ResolvedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

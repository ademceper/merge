using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Support Ticket Resolved Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SupportTicketResolvedEvent(
    Guid TicketId,
    string TicketNumber,
    Guid UserId,
    DateTime ResolvedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


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

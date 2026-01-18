using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record InvoiceOverdueEvent(
    Guid InvoiceId,
    Guid OrderId,
    string InvoiceNumber,
    DateTime DueDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

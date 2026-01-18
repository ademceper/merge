using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record CustomsDeclarationCreatedEvent(
    Guid DeclarationId,
    Guid OrderId,
    string DeclarationNumber,
    decimal TotalValue,
    string Currency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ProductDeletedEvent(
    Guid ProductId,
    string Name,
    string SKU) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Review Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReviewDeletedEvent(
    Guid ReviewId,
    Guid UserId,
    Guid ProductId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

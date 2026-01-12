using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Review Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReviewUpdatedEvent(
    Guid ReviewId,
    Guid UserId,
    Guid ProductId,
    int OldRating,
    int NewRating) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

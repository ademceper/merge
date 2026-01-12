namespace Merge.Domain.SharedKernel;

/// <summary>
/// Marker interface for aggregate roots - BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// Aggregate roots are the only objects that can be directly loaded from the repository.
/// All other objects in the aggregate must be accessed through the aggregate root.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Domain events raised by this aggregate root
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clear all domain events after they have been dispatched
    /// </summary>
    void ClearDomainEvents();
}

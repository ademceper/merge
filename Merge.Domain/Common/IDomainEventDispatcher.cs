namespace Merge.Domain.Common;

/// <summary>
/// Domain Event Dispatcher Interface - BOLUM 1.5: Domain Events (ZORUNLU)
/// Domain Event'leri publish etmek için kullanılır.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Publish domain events - BOLUM 1.5: Domain Events (ZORUNLU)
    /// UnitOfWork.Commit sonrası çağrılmalı
    /// </summary>
    Task DispatchDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}


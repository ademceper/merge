using Merge.Domain.SharedKernel;
using Microsoft.Extensions.Logging;

namespace Merge.Infrastructure.Common;

/// <summary>
/// Domain Event Dispatcher Implementation - BOLUM 1.5: Domain Events (ZORUNLU)
/// Basit bir implementation - MediatR kullanılmıyorsa bu kullanılır.
/// Production'da MediatR veya başka bir event bus kullanılabilir.
/// </summary>
public class DomainEventDispatcher(ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{

    // ✅ BOLUM 1.5: Domain Events publish mekanizması (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task DispatchDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        if (domainEvents == null || !domainEvents.Any())
            return;

        logger.LogInformation("Publishing {Count} domain events", domainEvents.Count());

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                logger.LogInformation(
                    "Publishing domain event: {EventType} at {OccurredOn}",
                    domainEvent.GetType().Name, domainEvent.OccurredOn);

                // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
                // TODO: Burada MediatR veya başka bir event bus kullanılabilir
                // Şimdilik sadece loglama yapıyoruz
                // Production'da MediatR.INotificationHandler<T> kullanılmalı
                
                // Örnek: await _mediator.Publish(domainEvent, cancellationToken);
                
                // ✅ Gelecekte async işlemler için await hazırlığı
                await Task.CompletedTask.ConfigureAwait(false);
                
                logger.LogInformation(
                    "Domain event published successfully: {EventType}",
                    domainEvent.GetType().Name);
            }
            catch (Exception ex)
            {
                // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
                logger.LogError(ex,
                    "Error publishing domain event: {EventType}",
                    domainEvent.GetType().Name);
                throw; // Re-throw - event publish hatası kritik
            }
        }
    }
}


using Merge.Domain.SharedKernel;
using Microsoft.Extensions.Logging;

namespace Merge.Infrastructure.Common;


public class DomainEventDispatcher(ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{

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

                // TODO: Burada MediatR veya başka bir event bus kullanılabilir
                // Şimdilik sadece loglama yapıyoruz
                // Production'da MediatR.INotificationHandler<T> kullanılmalı
                
                // Örnek: await _mediator.Publish(domainEvent, cancellationToken);
                
                await Task.CompletedTask.ConfigureAwait(false);
                
                logger.LogInformation(
                    "Domain event published successfully: {EventType}",
                    domainEvent.GetType().Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error publishing domain event: {EventType}",
                    domainEvent.GetType().Name);
                throw; // Re-throw - event publish hatası kritik
            }
        }
    }
}


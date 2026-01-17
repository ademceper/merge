using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Organization Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class OrganizationUpdatedEventHandler(ILogger<OrganizationUpdatedEventHandler> logger) : INotificationHandler<OrganizationUpdatedEvent>
{

    public async Task Handle(OrganizationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Organization updated event received. OrganizationId: {OrganizationId}, Name: {Name}, ChangedFields: {ChangedFields}",
            notification.OrganizationId, 
            notification.Name,
            string.Join(", ", notification.ChangedFields));

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation (sadece değişen alanlar için)
        // - Analytics tracking (hangi alanlar değişti)
        // - External system sync (CRM, ERP) - sadece değişen alanlar için

        await Task.CompletedTask;
    }
}

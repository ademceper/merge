using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// NotificationTemplate Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record NotificationTemplateDeletedEvent(
    Guid TemplateId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

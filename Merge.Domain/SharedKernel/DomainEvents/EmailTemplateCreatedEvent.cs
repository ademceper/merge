using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailTemplate Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailTemplateCreatedEvent(
    Guid TemplateId,
    string Name,
    EmailTemplateType Type) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

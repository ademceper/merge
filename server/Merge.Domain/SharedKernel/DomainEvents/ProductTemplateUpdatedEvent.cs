using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductTemplateUpdatedEvent(
    Guid TemplateId,
    string Name
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

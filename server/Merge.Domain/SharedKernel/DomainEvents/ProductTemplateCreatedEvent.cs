using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

// ✅ BOLUM 1.5: Domain Events (ZORUNLU)
public record ProductTemplateCreatedEvent(
    Guid TemplateId,
    string Name,
    Guid CategoryId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

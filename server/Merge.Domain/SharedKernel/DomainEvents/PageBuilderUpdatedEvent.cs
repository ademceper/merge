using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Page Builder Updated Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record PageBuilderUpdatedEvent(
    Guid PageBuilderId,
    string Name,
    string Slug) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


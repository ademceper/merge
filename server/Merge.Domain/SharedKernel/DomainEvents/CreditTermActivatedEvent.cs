using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Credit Term Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CreditTermActivatedEvent(
    Guid CreditTermId,
    Guid OrganizationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

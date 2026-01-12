using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Credit Term Credit Released Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CreditTermCreditReleasedEvent(
    Guid CreditTermId,
    Guid OrganizationId,
    decimal Amount,
    decimal UsedCredit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

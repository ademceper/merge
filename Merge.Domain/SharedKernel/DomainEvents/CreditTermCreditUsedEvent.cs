using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Credit Term Credit Used Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CreditTermCreditUsedEvent(
    Guid CreditTermId,
    Guid OrganizationId,
    decimal Amount,
    decimal UsedCredit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

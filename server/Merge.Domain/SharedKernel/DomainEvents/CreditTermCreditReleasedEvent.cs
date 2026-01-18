using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record CreditTermCreditReleasedEvent(
    Guid CreditTermId,
    Guid OrganizationId,
    decimal Amount,
    decimal UsedCredit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

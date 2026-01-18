using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record B2BUserCreditReleasedEvent(
    Guid B2BUserId,
    Guid UserId,
    Guid OrganizationId,
    decimal Amount,
    decimal UsedCredit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

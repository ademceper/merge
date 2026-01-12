using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// B2B User Credit Released Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record B2BUserCreditReleasedEvent(
    Guid B2BUserId,
    Guid UserId,
    Guid OrganizationId,
    decimal Amount,
    decimal UsedCredit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

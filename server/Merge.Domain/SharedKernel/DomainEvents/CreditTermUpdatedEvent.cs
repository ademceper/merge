using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record CreditTermUpdatedEvent(
    Guid CreditTermId,
    Guid OrganizationId,
    string Name,
    int PaymentDays,
    decimal? CreditLimit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

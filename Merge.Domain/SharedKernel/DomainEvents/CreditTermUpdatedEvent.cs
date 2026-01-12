using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Credit Term Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CreditTermUpdatedEvent(
    Guid CreditTermId,
    Guid OrganizationId,
    string Name,
    int PaymentDays,
    decimal? CreditLimit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

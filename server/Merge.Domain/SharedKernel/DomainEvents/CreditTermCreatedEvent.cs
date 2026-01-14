using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Credit Term Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CreditTermCreatedEvent(
    Guid CreditTermId,
    Guid OrganizationId,
    string Name,
    int PaymentDays,
    decimal? CreditLimit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

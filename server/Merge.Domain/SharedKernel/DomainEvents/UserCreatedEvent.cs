using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// User Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserCreatedEvent(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

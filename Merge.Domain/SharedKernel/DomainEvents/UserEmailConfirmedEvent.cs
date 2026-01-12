using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// User Email Confirmed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserEmailConfirmedEvent(
    Guid UserId,
    string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

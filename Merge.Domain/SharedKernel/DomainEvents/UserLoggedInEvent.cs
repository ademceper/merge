using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// User Logged In Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserLoggedInEvent(
    Guid UserId,
    string Email,
    string? IpAddress) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


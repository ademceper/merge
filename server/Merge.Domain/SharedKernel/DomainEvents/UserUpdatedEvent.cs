using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record UserUpdatedEvent(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

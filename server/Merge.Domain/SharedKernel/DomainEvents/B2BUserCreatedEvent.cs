using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record B2BUserCreatedEvent(
    Guid B2BUserId,
    Guid UserId,
    Guid OrganizationId,
    string? EmployeeId,
    string? Department,
    string? JobTitle,
    decimal? CreditLimit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

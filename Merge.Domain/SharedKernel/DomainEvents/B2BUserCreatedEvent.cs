using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// B2B User Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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

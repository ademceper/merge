using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReviewRejectedEvent(
    Guid ReviewId,
    Guid UserId, // Review owner
    Guid ProductId,
    Guid RejectedByUserId, // Admin who rejected
    string? Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

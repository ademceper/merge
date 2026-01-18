using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReviewApprovedEvent(
    Guid ReviewId,
    Guid UserId, // Review owner
    Guid ProductId,
    int Rating,
    Guid ApprovedByUserId) : IDomainEvent // Admin who approved
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

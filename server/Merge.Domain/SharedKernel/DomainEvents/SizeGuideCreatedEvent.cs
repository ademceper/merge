using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record SizeGuideCreatedEvent(
    Guid SizeGuideId,
    string Name,
    Guid CategoryId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

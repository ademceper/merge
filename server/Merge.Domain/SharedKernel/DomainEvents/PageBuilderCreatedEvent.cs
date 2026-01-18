using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PageBuilderCreatedEvent(
    Guid PageBuilderId,
    string Name,
    string Slug,
    Guid AuthorId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


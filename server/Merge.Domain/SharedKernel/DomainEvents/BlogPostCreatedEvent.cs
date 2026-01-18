using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record BlogPostCreatedEvent(
    Guid PostId,
    string Title,
    string Slug,
    Guid AuthorId,
    Guid CategoryId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


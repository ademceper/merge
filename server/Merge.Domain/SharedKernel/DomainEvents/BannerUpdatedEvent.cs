using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record BannerUpdatedEvent(
    Guid BannerId,
    string Title,
    string Position) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Banner Updated Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BannerUpdatedEvent(
    Guid BannerId,
    string Title,
    string Position) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


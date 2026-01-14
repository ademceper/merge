using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Banner Created Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BannerCreatedEvent(
    Guid BannerId,
    string Title,
    string Position) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


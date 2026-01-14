using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Banner Deleted Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BannerDeletedEvent(
    Guid BannerId,
    string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


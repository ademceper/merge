using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Landing Page Created Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record LandingPageCreatedEvent(
    Guid LandingPageId,
    string Name,
    string Slug,
    Guid AuthorId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}


using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Answer Helpfulness Marked Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AnswerHelpfulnessMarkedEvent(
    Guid AnswerId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

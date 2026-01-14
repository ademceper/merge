using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Question Helpfulness Marked Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record QuestionHelpfulnessMarkedEvent(
    Guid QuestionId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

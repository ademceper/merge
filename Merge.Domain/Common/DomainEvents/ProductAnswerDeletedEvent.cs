using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

// ✅ BOLUM 1.5: Domain Events (ZORUNLU)
public record ProductAnswerDeletedEvent(
    Guid AnswerId,
    Guid QuestionId,
    Guid UserId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

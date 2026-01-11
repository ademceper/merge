using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

// ✅ BOLUM 1.5: Domain Events (ZORUNLU)
public record ProductQuestionCreatedEvent(
    Guid QuestionId,
    Guid ProductId,
    Guid UserId,
    string Question
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

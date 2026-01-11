using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

// ✅ BOLUM 1.5: Domain Events (ZORUNLU)
public record ProductAnswerCreatedEvent(
    Guid AnswerId,
    Guid QuestionId,
    Guid UserId,
    bool IsSellerAnswer
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

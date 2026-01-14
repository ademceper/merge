using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

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

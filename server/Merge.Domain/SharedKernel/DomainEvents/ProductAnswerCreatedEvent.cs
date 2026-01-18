using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductAnswerCreatedEvent(
    Guid AnswerId,
    Guid QuestionId,
    Guid UserId,
    bool IsSellerAnswer
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

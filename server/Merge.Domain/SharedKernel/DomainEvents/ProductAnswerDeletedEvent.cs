using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductAnswerDeletedEvent(
    Guid AnswerId,
    Guid QuestionId,
    Guid UserId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

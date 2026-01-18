using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductAnswerUpdatedEvent(
    Guid AnswerId,
    Guid QuestionId,
    Guid UserId,
    int HelpfulCount
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

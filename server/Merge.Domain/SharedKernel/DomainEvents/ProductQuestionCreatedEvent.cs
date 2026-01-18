using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductQuestionCreatedEvent(
    Guid QuestionId,
    Guid ProductId,
    Guid UserId,
    string Question
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

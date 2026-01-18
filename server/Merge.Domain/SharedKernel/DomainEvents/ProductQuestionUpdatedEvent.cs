using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductQuestionUpdatedEvent(
    Guid QuestionId,
    Guid ProductId,
    Guid UserId,
    int AnswerCount,
    int HelpfulCount,
    bool HasSellerAnswer
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

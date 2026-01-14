using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

// ✅ BOLUM 1.5: Domain Events (ZORUNLU)
public record ProductAnswerApprovedEvent(
    Guid AnswerId,
    Guid QuestionId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

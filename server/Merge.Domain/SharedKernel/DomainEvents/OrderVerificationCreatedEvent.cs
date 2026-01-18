using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OrderVerificationCreatedEvent(
    Guid VerificationId,
    Guid OrderId,
    VerificationType VerificationType,
    int RiskScore,
    bool RequiresManualReview) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

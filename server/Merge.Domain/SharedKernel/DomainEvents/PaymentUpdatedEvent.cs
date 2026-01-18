using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PaymentUpdatedEvent(
    Guid PaymentId,
    Guid OrderId,
    string? TransactionId,
    string? PaymentReference,
    string? Metadata) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PreOrder Deposit Paid Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PreOrderDepositPaidEvent(Guid PreOrderId, Guid UserId, decimal DepositAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

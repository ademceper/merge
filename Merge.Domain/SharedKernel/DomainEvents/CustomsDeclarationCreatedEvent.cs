using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CustomsDeclaration Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CustomsDeclarationCreatedEvent(
    Guid DeclarationId,
    Guid OrderId,
    string DeclarationNumber,
    decimal TotalValue,
    string Currency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

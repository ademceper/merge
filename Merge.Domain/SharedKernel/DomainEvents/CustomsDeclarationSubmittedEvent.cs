using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CustomsDeclaration Submitted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CustomsDeclarationSubmittedEvent(
    Guid DeclarationId,
    Guid OrderId,
    string DeclarationNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CustomsDeclaration Rejected Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CustomsDeclarationRejectedEvent(
    Guid DeclarationId,
    Guid OrderId,
    string DeclarationNumber,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

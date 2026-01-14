using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CustomsDeclaration Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CustomsDeclarationUpdatedEvent(
    Guid DeclarationId,
    Guid OrderId,
    string UpdatedField) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

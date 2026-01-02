namespace Merge.Domain.Common;

/// <summary>
/// Domain Event interface - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}


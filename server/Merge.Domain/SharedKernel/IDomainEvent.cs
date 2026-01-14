using MediatR;

namespace Merge.Domain.SharedKernel;

/// <summary>
/// Domain Event interface - BOLUM 1.5: Domain Events (ZORUNLU)
/// MediatR INotification'dan türer - Event handler'lar için uyumluluk
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}


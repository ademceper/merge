using MediatR;

namespace Merge.Domain.Common;

/// <summary>
/// Domain Event interface - BOLUM 1.5: Domain Events (ZORUNLU)
/// MediatR INotification'dan türer - Event handler'lar için uyumluluk
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}


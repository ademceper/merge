using Merge.Domain.Entities;

namespace Merge.Domain.SharedKernel;

/// <summary>
/// Base Aggregate Root - BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// Aggregate root'lar için base class.
/// BaseEntity'den türeyen ve IAggregateRoot interface'ini implement eden entity'ler için kullanılır.
/// </summary>
public abstract class BaseAggregateRoot : BaseEntity, IAggregateRoot
{
    // BaseEntity'de zaten DomainEvents, AddDomainEvent(), ClearDomainEvents() var
    // Bu class sadece marker olarak kullanılıyor - IAggregateRoot implement ediyor
}


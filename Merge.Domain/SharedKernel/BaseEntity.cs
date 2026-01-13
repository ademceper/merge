using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel;

/// <summary>
/// Base Entity with Domain Events support - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    // ✅ BOLUM 1.1: Rich Domain Model - Virtual properties for override (encapsulation)
    // NOT: EF Core için public set gerekli, ancak derived class'larda override edilip private set yapılabilir
    public virtual Guid Id { get; protected set; } = Guid.NewGuid();
    public virtual DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; protected set; }
    public virtual bool IsDeleted { get; protected set; } = false;

    /// <summary>
    /// Domain Events collection - Read-only access
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Add domain event - BOLUM 1.5: Domain Events (ZORUNLU)
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove domain event
    /// </summary>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Mark entity as deleted
    /// </summary>
    public virtual void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clear all domain events - Called after events are published
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}


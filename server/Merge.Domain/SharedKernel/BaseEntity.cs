using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel;


public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    // NOT: EF Core ve Infrastructure katmanı erişimi için public set gerekli, ancak derived class'larda override edilip private set yapılabilir
    public virtual Guid Id { get; set; } = Guid.NewGuid();
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual bool IsDeleted { get; set; } = false;

    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
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


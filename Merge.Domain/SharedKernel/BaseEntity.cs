using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel;

/// <summary>
/// Base Entity with Domain Events support - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

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
    /// Clear all domain events - Called after events are published
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}


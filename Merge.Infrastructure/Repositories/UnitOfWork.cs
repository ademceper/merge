using Microsoft.EntityFrameworkCore.Storage;
using Merge.Domain.Common;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;

namespace Merge.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventDispatcher? _domainEventDispatcher;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context, IDomainEventDispatcher? domainEventDispatcher = null)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
    }

    // ✅ BOLUM 1.5: Domain Events publish mekanizması (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Domain Event'leri topla (SaveChanges öncesi)
        var domainEvents = GetDomainEvents();

        // 2. Database'e kaydet
        var result = await _context.SaveChangesAsync(cancellationToken);

        // 3. Domain Event'leri publish et (SaveChanges sonrası - transaction commit edildikten sonra)
        if (domainEvents.Any() && _domainEventDispatcher != null)
        {
            await _domainEventDispatcher.DispatchDomainEventsAsync(domainEvents, cancellationToken);
            
            // 4. Event'leri temizle
            ClearDomainEvents();
        }

        return result;
    }

    /// <summary>
    /// Get all domain events from tracked entities - BOLUM 1.5: Domain Events
    /// </summary>
    private List<IDomainEvent> GetDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();

        var entities = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            domainEvents.AddRange(entity.DomainEvents);
        }

        return domainEvents;
    }

    /// <summary>
    /// Clear domain events from tracked entities - BOLUM 1.5: Domain Events
    /// </summary>
    private void ClearDomainEvents()
    {
        var entities = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}


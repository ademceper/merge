using Microsoft.EntityFrameworkCore.Storage;
using Merge.Domain.SharedKernel;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Application.Interfaces;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Merge.Infrastructure.Data;

namespace Merge.Infrastructure.Repositories;

// ✅ BOLUM 1.1: UnitOfWork Application katmanındaki IUnitOfWork interface'ini implement ediyor
public class UnitOfWork(ApplicationDbContext context, IDomainEventDispatcher? domainEventDispatcher) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    // ✅ BOLUM 3.0: Outbox pattern (dual-write sorunu çözümü)
    // ✅ BOLUM 1.5: Domain Events publish mekanizması (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get all domain events from tracked entities
        var domainEvents = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // Convert domain events to outbox messages (same transaction)
        foreach (var domainEvent in domainEvents)
        {
            context.Set<OutboxMessage>().Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                Content = System.Text.Json.JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredOnUtc = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Clear domain events
        foreach (var entry in context.ChangeTracker.Entries<IAggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return await context.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
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
        context.Dispose();
    }
}


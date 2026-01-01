namespace Merge.Infrastructure.Repositories;

public interface IUnitOfWork : IDisposable
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}


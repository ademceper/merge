namespace Merge.Domain.Interfaces;

// ✅ BOLUM 1.1: Interface'ler Application katmanında olmalı (Clean Architecture)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IUnitOfWork : IDisposable
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}


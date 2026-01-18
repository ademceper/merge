using Microsoft.EntityFrameworkCore;
using Merge.Domain.Entities;

namespace Merge.Domain.Interfaces;

// ⚠️ NOT: ApplicationDbContext'i IDbContext olarak kullanmak için cast gerekiyor
// Gelecekte tüm service'ler Set<T>() metodunu kullanmalı
public interface IDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker { get; }
    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
}


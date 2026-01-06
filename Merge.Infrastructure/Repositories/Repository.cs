using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Entities;
using Merge.Domain.Specifications;
using Merge.Application.Interfaces;
using Merge.Infrastructure.Data;

namespace Merge.Infrastructure.Repositories;

// ✅ BOLUM 1.1: Repository Application katmanındaki IRepository interface'ini implement ediyor
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ⚠️ NOT: AsNoTracking() YOK - Entity track edilmeli (update işlemleri için)
    // Read-only query'ler için Specification Pattern kullanılmalı (BOLUM 7.2)
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        // ✅ PERFORMANCE: Removed manual !IsDeleted check - Global Query Filter handles this automatically
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.1: AsNoTracking for read-only queries (ZORUNLUDUR)
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        // ✅ PERFORMANCE: Removed manual !IsDeleted check - Global Query Filter handles this automatically
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // ⚠️ DEPRECATED: Specification Pattern implement edildikten sonra kaldırılacak
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.1: AsNoTracking for read-only queries (ZORUNLUDUR)
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        // ✅ PERFORMANCE: Removed manual !IsDeleted check - Global Query Filter handles this automatically
        return await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    // ⚠️ DEPRECATED: Specification Pattern implement edildikten sonra kaldırılacak
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.1: AsNoTracking for read-only queries (ZORUNLUDUR)
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        // ✅ PERFORMANCE: Removed manual !IsDeleted check - Global Query Filter handles this automatically
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity, cancellationToken);

        // ✅ CRITICAL FIX: Removed SaveChangesAsync call
        // This allows Unit of Work pattern to work correctly
        // Service layer must call SaveChanges explicitly via UnitOfWork
        // BEFORE: Each Add commits immediately (breaks transactions)
        // AFTER: Changes tracked, committed when UnitOfWork.SaveChanges() called

        return entity;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);

        // ✅ CRITICAL FIX: Removed SaveChangesAsync call
        // Enables atomic multi-entity operations via Unit of Work
        return Task.CompletedTask;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE FIX: Soft delete implementation
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);

        // ✅ CRITICAL FIX: Removed SaveChangesAsync call
        // Note: Changed from UpdateAsync(entity) to direct Update()
        // to avoid recursive SaveChanges issue
        return Task.CompletedTask;
    }

    // ⚠️ DEPRECATED: Specification Pattern implement edildikten sonra kaldırılacak
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.1: AsNoTracking for read-only queries (ZORUNLUDUR)
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        // ✅ PERFORMANCE: Removed manual !IsDeleted check - Global Query Filter handles this automatically
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(predicate, cancellationToken);
    }

    // ⚠️ DEPRECATED: Specification Pattern implement edildikten sonra kaldırılacak
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.1: AsNoTracking for read-only queries (ZORUNLUDUR)
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        // ✅ PERFORMANCE: Removed manual !IsDeleted check - Global Query Filter handles this automatically
        var query = _dbSet
            .AsNoTracking()
            .AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync(cancellationToken);
    }

    // ✅ BOLUM 7.2: Specification Pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public virtual async Task<T?> GetBySpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    // ✅ BOLUM 7.2: Specification Pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public virtual async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    // ✅ BOLUM 7.2: Specification Pattern (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public virtual async Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec, skipPaging: true).CountAsync(cancellationToken);
    }

    /// <summary>
    /// Apply specification to query - BOLUM 7.2: Specification Pattern
    /// </summary>
    private IQueryable<T> ApplySpecification(ISpecification<T> spec, bool skipPaging = false)
    {
        var query = _dbSet.AsQueryable();

        // ✅ BOLUM 6.1: AsNoTracking for read-only queries (ZORUNLUDUR)
        if (spec.IsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply criteria (Where clause)
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        // Apply includes
        query = spec.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes (for complex paths like "Order.OrderItems.Product")
        query = spec.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // Apply pagination (skip if counting)
        if (!skipPaging)
        {
            if (spec.Skip > 0)
            {
                query = query.Skip(spec.Skip);
            }

            if (spec.Take > 0)
            {
                query = query.Take(spec.Take);
            }
        }

        return query;
    }
}


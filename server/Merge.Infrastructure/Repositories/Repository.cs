using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Entities;
using Merge.Domain.SharedKernel;
using Merge.Domain.Specifications;
using Merge.Application.Interfaces;
using Merge.Infrastructure.Data;

namespace Merge.Infrastructure.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    // ⚠️ NOT: AsNoTracking() YOK - Entity track edilmeli (update işlemleri için)
    // Read-only query'ler için Specification Pattern kullanılmalı (BOLUM 7.2)
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // ⚠️ DEPRECATED: Specification Pattern implement edildikten sonra kaldırılacak
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    // ⚠️ DEPRECATED: Specification Pattern implement edildikten sonra kaldırılacak
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity, cancellationToken);

        // This allows Unit of Work pattern to work correctly
        // Service layer must call SaveChanges explicitly via UnitOfWork
        // BEFORE: Each Add commits immediately (breaks transactions)
        // AFTER: Changes tracked, committed when UnitOfWork.SaveChanges() called

        return entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);

        // Enables atomic multi-entity operations via Unit of Work
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);

        // Note: Changed from UpdateAsync(entity) to direct Update()
        // to avoid recursive SaveChanges issue
        return Task.CompletedTask;
    }

    // ⚠️ DEPRECATED: Specification Pattern implement edildikten sonra kaldırılacak
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(predicate, cancellationToken);
    }

    // ⚠️ DEPRECATED: Specification Pattern implement edildikten sonra kaldırılacak
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .AsQueryable();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync(cancellationToken);
    }

    public virtual async Task<T?> GetBySpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec, skipPaging: true).CountAsync(cancellationToken);
    }

    
    private IQueryable<T> ApplySpecification(ISpecification<T> spec, bool skipPaging = false)
    {
        var query = _dbSet.AsQueryable();

        if (spec.IsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply criteria (Where clause)
        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }

        var includeCount = (spec.Includes?.Count ?? 0) + (spec.IncludeStrings?.Count ?? 0);
        if (includeCount >= 2)
        {
            query = query.AsSplitQuery();
        }

        // Apply includes
        query = spec.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes (for complex paths like "Order.OrderItems.Product")
        query = spec.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (spec.OrderBy is not null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending is not null)
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


using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;

namespace Merge.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE FIX: Removed manual !IsDeleted check
        // Global Query Filter handles this automatically
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        // ✅ PERFORMANCE FIX: Removed manual !IsDeleted check
        // Global Query Filter handles this automatically
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        // ✅ PERFORMANCE FIX: Removed manual !IsDeleted check
        // Global Query Filter handles this automatically
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        // ✅ PERFORMANCE FIX: Removed manual !IsDeleted check
        // Global Query Filter handles this automatically
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);

        // ✅ CRITICAL FIX: Removed SaveChangesAsync call
        // This allows Unit of Work pattern to work correctly
        // Service layer must call SaveChanges explicitly via UnitOfWork
        // BEFORE: Each Add commits immediately (breaks transactions)
        // AFTER: Changes tracked, committed when UnitOfWork.SaveChanges() called

        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);

        // ✅ CRITICAL FIX: Removed SaveChangesAsync call
        // Enables atomic multi-entity operations via Unit of Work
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
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

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        // ✅ PERFORMANCE FIX: Removed manual !IsDeleted check
        // Global Query Filter handles this automatically
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        // ✅ PERFORMANCE FIX: Removed manual !IsDeleted check
        // Global Query Filter handles this automatically
        var query = _dbSet.AsQueryable();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.CountAsync();
    }
}


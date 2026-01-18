using System.Linq.Expressions;
using Merge.Domain.Entities;
using Merge.Domain.Specifications;
using Merge.Domain.SharedKernel;
using Merge.Domain.Interfaces;

namespace Merge.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<T?> GetBySpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    
    // ⚠️ DEPRECATED: Specification Pattern kullanılmalı - BOLUM 7.2
    // Geçici olarak geriye dönük uyumluluk için mevcut
    // Yeni kod yazarken Specification Pattern kullanın
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    [Obsolete("Use Specification Pattern instead. This method will be removed in future versions.")]
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}


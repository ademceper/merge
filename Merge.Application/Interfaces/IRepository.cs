using System.Linq.Expressions;
using Merge.Domain.Entities;
using Merge.Domain.Specifications;

namespace Merge.Application.Interfaces;

// ✅ BOLUM 1.1: Interface'ler Application katmanında olmalı (Clean Architecture)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 7.2: Specification Pattern (ZORUNLU)
public interface IRepository<T> where T : BaseEntity
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    
    // ✅ BOLUM 7.2: Specification Pattern (ZORUNLU)
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


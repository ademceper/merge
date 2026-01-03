using System.Linq.Expressions;

namespace Merge.Domain.Specifications;

/// <summary>
/// Specification Pattern Interface - BOLUM 7.2: Specification Pattern (ZORUNLU)
/// Generic Repository Anti-Pattern çözümü için kullanılır.
/// LINQ expression'ları repository'den dışa sızdırmaz, Include/ThenInclude desteği sağlar.
/// </summary>
public interface ISpecification<T>
{
    /// <summary>
    /// Where clause criteria
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Include expressions for eager loading
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// ThenInclude expressions for nested eager loading
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// OrderBy expression
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// OrderByDescending expression
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Take (limit) value for pagination
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Skip (offset) value for pagination
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Whether to use AsNoTracking for read-only queries
    /// </summary>
    bool IsNoTracking { get; }
}


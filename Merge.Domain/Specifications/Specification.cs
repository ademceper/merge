using System.Linq.Expressions;

namespace Merge.Domain.Specifications;

/// <summary>
/// Specification Pattern Base Class - BOLUM 7.2: Specification Pattern (ZORUNLU)
/// Generic Repository Anti-Pattern çözümü için kullanılır.
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    protected Specification()
    {
        Includes = new List<Expression<Func<T, object>>>();
        IncludeStrings = new List<string>();
        IsNoTracking = true; // ✅ BOLUM 6.1: Default olarak AsNoTracking (read-only queries için)
    }

    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; }
    public List<string> IncludeStrings { get; }
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int Take { get; protected set; }
    public int Skip { get; protected set; }
    public bool IsNoTracking { get; protected set; }

    /// <summary>
    /// Add Include expression for eager loading
    /// </summary>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Add Include string for complex includes (e.g., "Order.OrderItems.Product")
    /// </summary>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Set pagination
    /// </summary>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Set order by
    /// </summary>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
        OrderByDescending = null;
    }

    /// <summary>
    /// Set order by descending
    /// </summary>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
        OrderBy = null;
    }

    /// <summary>
    /// Enable tracking (for update operations)
    /// </summary>
    protected void EnableTracking()
    {
        IsNoTracking = false;
    }
}


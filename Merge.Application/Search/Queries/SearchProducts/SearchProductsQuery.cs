using MediatR;
using Merge.Application.DTOs.Search;

namespace Merge.Application.Search.Queries.SearchProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SearchProductsQuery(
    string? SearchTerm = null,
    Guid? CategoryId = null,
    string? Brand = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    decimal? MinRating = null,
    bool InStockOnly = false,
    string? SortBy = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<SearchResultDto>;

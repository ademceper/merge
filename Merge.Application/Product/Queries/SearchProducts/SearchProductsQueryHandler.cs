using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Product.Queries.SearchProducts;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, PagedResult<ProductDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchProductsQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public SearchProductsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<SearchProductsQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize
            ? _paginationSettings.MaxPageSize
            : request.PageSize;

        // PERFORMANCE: EF.Functions.ILike for case-insensitive search with PostgreSQL
        // PERFORMANCE: AsNoTracking for read-only queries
        var query = _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive &&
                (EF.Functions.ILike(p.Name, $"%{request.SearchTerm}%") ||
                 EF.Functions.ILike(p.Description, $"%{request.SearchTerm}%") ||
                 EF.Functions.ILike(p.Brand, $"%{request.SearchTerm}%")));

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Product search completed. Term: {SearchTerm}, Results: {Count}, TotalCount: {TotalCount}",
            request.SearchTerm, products.Count, totalCount);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return new PagedResult<ProductDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

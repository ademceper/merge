using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Product.Queries.GetAllProducts;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly PaginationSettings _paginationSettings;

    public GetAllProductsQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize
            ? _paginationSettings.MaxPageSize
            : request.PageSize;

        // PERFORMANCE: AsNoTracking for read-only queries
        var query = _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

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

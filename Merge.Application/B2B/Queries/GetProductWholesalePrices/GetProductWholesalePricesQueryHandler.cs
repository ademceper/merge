using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetProductWholesalePrices;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetProductWholesalePricesQueryHandler : IRequestHandler<GetProductWholesalePricesQuery, PagedResult<WholesalePriceDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductWholesalePricesQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetProductWholesalePricesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetProductWholesalePricesQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<WholesalePriceDto>> Handle(GetProductWholesalePricesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: AsSplitQuery to avoid Cartesian Explosion (multiple Include'lar)
        // ✅ PERFORMANCE: Removed manual !wp.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<WholesalePrice>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting - Multiple Include'lar için
            .Include(wp => wp.Product)
            .Include(wp => wp.Organization)
            .Where(wp => wp.ProductId == request.ProductId && wp.IsActive);

        if (request.OrganizationId.HasValue)
        {
            query = query.Where(wp => wp.OrganizationId == request.OrganizationId.Value || wp.OrganizationId == null);
        }
        else
        {
            query = query.Where(wp => wp.OrganizationId == null); // General pricing
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var prices = await query
            .OrderBy(wp => wp.MinQuantity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<WholesalePriceDto>>(prices);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<WholesalePriceDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}


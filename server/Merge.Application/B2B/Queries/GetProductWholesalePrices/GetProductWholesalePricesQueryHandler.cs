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

public class GetProductWholesalePricesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetProductWholesalePricesQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetProductWholesalePricesQuery, PagedResult<WholesalePriceDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<WholesalePriceDto>> Handle(GetProductWholesalePricesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<WholesalePrice>()
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

        var totalCount = await query.CountAsync(cancellationToken);

        var prices = await query
            .OrderBy(wp => wp.MinQuantity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<WholesalePriceDto>>(prices);

        return new PagedResult<WholesalePriceDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}


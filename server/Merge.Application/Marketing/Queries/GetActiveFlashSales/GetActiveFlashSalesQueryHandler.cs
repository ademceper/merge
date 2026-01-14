using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetActiveFlashSales;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetActiveFlashSalesQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetActiveFlashSalesQuery, PagedResult<FlashSaleDto>>
{
    public async Task<PagedResult<FlashSaleDto>> Handle(GetActiveFlashSalesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: AsSplitQuery - N+1 query önleme (Cartesian Explosion önleme)
        var query = context.Set<FlashSale>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .Where(fs => fs.IsActive && fs.StartDate <= now && fs.EndDate >= now)
            .OrderByDescending(fs => fs.StartDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var flashSales = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FlashSaleDto>
        {
            Items = mapper.Map<List<FlashSaleDto>>(flashSales),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

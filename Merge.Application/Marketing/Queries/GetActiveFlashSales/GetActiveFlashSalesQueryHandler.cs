using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetActiveFlashSales;

public class GetActiveFlashSalesQueryHandler : IRequestHandler<GetActiveFlashSalesQuery, PagedResult<FlashSaleDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetActiveFlashSalesQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<FlashSaleDto>> Handle(GetActiveFlashSalesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: AsSplitQuery - N+1 query önleme (Cartesian Explosion önleme)
        var query = _context.Set<FlashSale>()
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
            Items = _mapper.Map<List<FlashSaleDto>>(flashSales),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

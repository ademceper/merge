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

namespace Merge.Application.Marketing.Queries.GetAllFlashSales;

public class GetAllFlashSalesQueryHandler : IRequestHandler<GetAllFlashSalesQuery, PagedResult<FlashSaleDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetAllFlashSalesQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<FlashSaleDto>> Handle(GetAllFlashSalesQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery - N+1 query önleme (Cartesian Explosion önleme)
        var query = _context.Set<FlashSale>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .OrderByDescending(fs => fs.CreatedAt);

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

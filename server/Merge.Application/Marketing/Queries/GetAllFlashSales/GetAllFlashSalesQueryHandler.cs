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

public class GetAllFlashSalesQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetAllFlashSalesQuery, PagedResult<FlashSaleDto>>
{
    public async Task<PagedResult<FlashSaleDto>> Handle(GetAllFlashSalesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Set<FlashSale>()
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
            Items = mapper.Map<List<FlashSaleDto>>(flashSales),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

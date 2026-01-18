using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetAllCommissions;

public class GetAllCommissionsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllCommissionsQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllCommissionsQuery, PagedResult<SellerCommissionDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<SellerCommissionDto>> Handle(GetAllCommissionsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all commissions. Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.Status?.ToString() ?? "All", request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize 
            ? paginationConfig.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<SellerCommission> query = context.Set<SellerCommission>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem);

        if (request.Status.HasValue)
        {
            query = query.Where(sc => sc.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var commissionDtos = mapper.Map<IEnumerable<SellerCommissionDto>>(commissions).ToList();

        return new PagedResult<SellerCommissionDto>
        {
            Items = commissionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

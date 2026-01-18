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
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetAllSellerApplications;

public class GetAllSellerApplicationsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllSellerApplicationsQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllSellerApplicationsQuery, PagedResult<SellerApplicationDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<SellerApplicationDto>> Handle(GetAllSellerApplicationsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all seller applications. Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.Status?.ToString() ?? "All", request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize 
            ? paginationConfig.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<SellerApplication> query = context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User);

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var applications = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var applicationDtos = mapper.Map<IEnumerable<SellerApplicationDto>>(applications).ToList();

        return new PagedResult<SellerApplicationDto>
        {
            Items = applicationDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

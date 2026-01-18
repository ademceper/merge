using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerProducts;

public class GetSellerProductsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSellerProductsQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetSellerProductsQuery, PagedResult<ProductDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<ProductDto>> Handle(GetSellerProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting seller products. SellerId: {SellerId}, Page: {Page}, PageSize: {PageSize}",
            request.SellerId, request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize 
            ? paginationConfig.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<ProductEntity> query = context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.SellerId == request.SellerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var productDtos = mapper.Map<IEnumerable<ProductDto>>(products).ToList();

        return new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

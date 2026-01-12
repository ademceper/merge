using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerOrdersQueryHandler : IRequestHandler<GetSellerOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerOrdersQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetSellerOrdersQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSellerOrdersQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetSellerOrdersQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting seller orders. SellerId: {SellerId}, Page: {Page}, PageSize: {PageSize}",
            request.SellerId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number config'den
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize 
            ? _paginationSettings.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        IQueryable<OrderEntity> query = _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId));

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders).ToList();

        return new PagedResult<OrderDto>
        {
            Items = orderDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

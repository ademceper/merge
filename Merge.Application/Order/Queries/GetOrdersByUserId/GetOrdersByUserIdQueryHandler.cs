using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Queries.GetOrdersByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetOrdersByUserIdQueryHandler : IRequestHandler<GetOrdersByUserIdQuery, PagedResult<OrderDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrdersByUserIdQueryHandler> _logger;
    private readonly OrderSettings _orderSettings;

    public GetOrdersByUserIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetOrdersByUserIdQueryHandler> logger,
        IOptions<OrderSettings> orderSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _orderSettings = orderSettings.Value;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Pagination
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = _context.Set<OrderEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Configuration'dan al
        var pageSize = request.PageSize > _orderSettings.MaxPageSize ? _orderSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;
        
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} orders (page {Page}) for user {UserId}",
            orders.Count, page, request.UserId);

        return new PagedResult<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(orders),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

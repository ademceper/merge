using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Order.Queries.GetOrdersByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetOrdersByUserIdQueryHandler : IRequestHandler<GetOrdersByUserIdQuery, PagedResult<OrderDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrdersByUserIdQueryHandler> _logger;

    public GetOrdersByUserIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetOrdersByUserIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Pagination
        var query = _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .Include(o => o.User)
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} orders (page {Page}) for user {UserId}",
            orders.Count, request.Page, request.UserId);

        return new PagedResult<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(orders),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

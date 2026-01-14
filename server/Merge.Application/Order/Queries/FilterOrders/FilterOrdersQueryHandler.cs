using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Queries.FilterOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class FilterOrdersQueryHandler : IRequestHandler<FilterOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly OrderSettings _orderSettings;

    public FilterOrdersQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<OrderSettings> orderSettings)
    {
        _context = context;
        _mapper = mapper;
        _orderSettings = orderSettings.Value;
    }

    public async Task<PagedResult<OrderDto>> Handle(FilterOrdersQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Configuration'dan al
        var pageSize = request.PageSize > _orderSettings.MaxPageSize ? _orderSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = _context.Set<OrderEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.User)
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            var statusEnum = Enum.Parse<OrderStatus>(request.Status);
            query = query.Where(o => o.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(request.PaymentStatus))
        {
            var paymentStatusEnum = Enum.Parse<PaymentStatus>(request.PaymentStatus);
            query = query.Where(o => o.PaymentStatus == paymentStatusEnum);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.EndDate.Value);
        }

        if (request.MinAmount.HasValue)
        {
            query = query.Where(o => o.TotalAmount >= request.MinAmount.Value);
        }

        if (request.MaxAmount.HasValue)
        {
            query = query.Where(o => o.TotalAmount <= request.MaxAmount.Value);
        }

        if (!string.IsNullOrEmpty(request.OrderNumber))
        {
            query = query.Where(o => EF.Functions.ILike(o.OrderNumber, $"%{request.OrderNumber}%"));
        }

        query = request.SortBy?.ToLower() switch
        {
            "amount" => request.SortDescending 
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),
            "status" => request.SortDescending
                ? query.OrderByDescending(o => o.Status)
                : query.OrderBy(o => o.Status),
            _ => request.SortDescending
                ? query.OrderByDescending(o => o.CreatedAt)
                : query.OrderBy(o => o.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(orders),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

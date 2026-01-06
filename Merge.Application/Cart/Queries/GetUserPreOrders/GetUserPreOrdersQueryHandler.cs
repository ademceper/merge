using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;

namespace Merge.Application.Cart.Queries.GetUserPreOrders;

public class GetUserPreOrdersQueryHandler : IRequestHandler<GetUserPreOrdersQuery, PagedResult<PreOrderDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetUserPreOrdersQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<PreOrderDto>> Handle(GetUserPreOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .Where(po => po.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var preOrders = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = _mapper.Map<List<PreOrderDto>>(preOrders);

        return new PagedResult<PreOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}


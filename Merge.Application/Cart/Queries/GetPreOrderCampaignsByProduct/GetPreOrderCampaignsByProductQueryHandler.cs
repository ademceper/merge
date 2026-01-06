using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;

namespace Merge.Application.Cart.Queries.GetPreOrderCampaignsByProduct;

public class GetPreOrderCampaignsByProductQueryHandler : IRequestHandler<GetPreOrderCampaignsByProductQuery, PagedResult<PreOrderCampaignDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetPreOrderCampaignsByProductQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<PreOrderCampaignDto>> Handle(GetPreOrderCampaignsByProductQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.ProductId == request.ProductId);

        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = _mapper.Map<List<PreOrderCampaignDto>>(campaigns);

        return new PagedResult<PreOrderCampaignDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}


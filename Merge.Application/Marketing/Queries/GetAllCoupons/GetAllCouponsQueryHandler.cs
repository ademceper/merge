using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetAllCoupons;

public class GetAllCouponsQueryHandler : IRequestHandler<GetAllCouponsQuery, PagedResult<CouponDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetAllCouponsQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<CouponDto>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Coupon>()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var coupons = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CouponDto>
        {
            Items = _mapper.Map<List<CouponDto>>(coupons),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

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
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CouponDto>
        {
            Items = _mapper.Map<List<CouponDto>>(coupons),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

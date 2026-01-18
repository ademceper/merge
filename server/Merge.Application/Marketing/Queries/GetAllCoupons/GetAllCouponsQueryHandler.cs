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

public class GetAllCouponsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetAllCouponsQuery, PagedResult<CouponDto>>
{
    public async Task<PagedResult<CouponDto>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Set<Coupon>()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var coupons = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CouponDto>
        {
            Items = mapper.Map<List<CouponDto>>(coupons),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetCouponByCode;

public class GetCouponByCodeQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetCouponByCodeQuery, CouponDto?>
{
    public async Task<CouponDto?> Handle(GetCouponByCodeQuery request, CancellationToken cancellationToken)
    {
        var coupon = await context.Set<Coupon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Code, request.Code), cancellationToken);

        return coupon is null ? null : mapper.Map<CouponDto>(coupon);
    }
}

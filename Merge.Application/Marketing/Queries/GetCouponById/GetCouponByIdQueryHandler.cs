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

namespace Merge.Application.Marketing.Queries.GetCouponById;

public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, CouponDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetCouponByIdQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CouponDto?> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Set<Coupon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return coupon == null ? null : _mapper.Map<CouponDto>(coupon);
    }
}

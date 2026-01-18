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

namespace Merge.Application.Marketing.Queries.GetMyReferrals;

public class GetMyReferralsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetMyReferralsQuery, PagedResult<ReferralDto>>
{
    public async Task<PagedResult<ReferralDto>> Handle(GetMyReferralsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Set<Referral>()
            .AsNoTracking()
            .Include(r => r.ReferredUser)
            .Where(r => r.ReferrerId == request.UserId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var referrals = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ReferralDto>
        {
            Items = mapper.Map<List<ReferralDto>>(referrals),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

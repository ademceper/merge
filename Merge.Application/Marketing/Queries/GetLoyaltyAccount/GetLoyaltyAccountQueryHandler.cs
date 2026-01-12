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

namespace Merge.Application.Marketing.Queries.GetLoyaltyAccount;

public class GetLoyaltyAccountQueryHandler : IRequestHandler<GetLoyaltyAccountQuery, LoyaltyAccountDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetLoyaltyAccountQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LoyaltyAccountDto?> Handle(GetLoyaltyAccountQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery - N+1 query önleme (Cartesian Explosion önleme)
        var account = await _context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Tier)
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

        return account == null ? null : _mapper.Map<LoyaltyAccountDto>(account);
    }
}

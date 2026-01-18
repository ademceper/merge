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

public class GetLoyaltyAccountQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetLoyaltyAccountQuery, LoyaltyAccountDto?>
{
    public async Task<LoyaltyAccountDto?> Handle(GetLoyaltyAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .Include(a => a.Tier)
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

        return account == null ? null : mapper.Map<LoyaltyAccountDto>(account);
    }
}

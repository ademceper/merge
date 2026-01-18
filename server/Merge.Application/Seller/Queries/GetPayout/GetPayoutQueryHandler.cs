using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetPayout;

public class GetPayoutQueryHandler(IDbContext context, IMapper mapper, ILogger<GetPayoutQueryHandler> logger) : IRequestHandler<GetPayoutQuery, CommissionPayoutDto?>
{

    public async Task<CommissionPayoutDto?> Handle(GetPayoutQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting payout. PayoutId: {PayoutId}", request.PayoutId);

        var payout = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

        return payout is not null ? mapper.Map<CommissionPayoutDto>(payout) : null;
    }
}

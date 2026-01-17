using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerBalance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerBalanceQueryHandler(IDbContext context, ILogger<GetSellerBalanceQueryHandler> logger) : IRequestHandler<GetSellerBalanceQuery, SellerBalanceDto>
{

    public async Task<SellerBalanceDto> Handle(GetSellerBalanceQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting seller balance. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await context.Set<SellerProfile>()
            .AsNoTracking()
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == request.SellerId, cancellationToken);

        if (seller == null)
        {
            logger.LogWarning("Seller not found. SellerId: {SellerId}", request.SellerId);
            throw new NotFoundException("Satıcı", request.SellerId);
        }

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // Calculate in-transit balance (payouts being processed)
        var inTransitBalance = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .Where(p => p.SellerId == request.SellerId && 
                   (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing))
            .SumAsync(p => p.TotalAmount, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // Calculate total payouts
        var totalPayouts = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .Where(p => p.SellerId == request.SellerId && 
                   p.Status == PayoutStatus.Completed)
            .SumAsync(p => p.NetAmount, cancellationToken);

        return new SellerBalanceDto
        {
            SellerId = request.SellerId,
            SellerName = seller.StoreName,
            TotalEarnings = seller.TotalEarnings,
            PendingBalance = seller.PendingBalance,
            AvailableBalance = seller.AvailableBalance,
            InTransitBalance = Math.Round(inTransitBalance, 2),
            TotalPayouts = Math.Round(totalPayouts, 2),
            NextPayoutDate = 0 // Would need payout schedule
        };
    }
}

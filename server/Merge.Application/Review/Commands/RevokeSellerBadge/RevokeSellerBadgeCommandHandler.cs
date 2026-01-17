using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.RevokeSellerBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RevokeSellerBadgeCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RevokeSellerBadgeCommandHandler> logger) : IRequestHandler<RevokeSellerBadgeCommand, bool>
{

    public async Task<bool> Handle(RevokeSellerBadgeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Revoking seller badge. SellerId: {SellerId}, BadgeId: {BadgeId}",
            request.SellerId, request.BadgeId);

        var badge = await context.Set<SellerTrustBadge>()
            .FirstOrDefaultAsync(stb => stb.SellerId == request.SellerId && stb.TrustBadgeId == request.BadgeId, cancellationToken);

        if (badge == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        badge.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seller badge revoked successfully. SellerId: {SellerId}, BadgeId: {BadgeId}",
            request.SellerId, request.BadgeId);
        return true;
    }
}

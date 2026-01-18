using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.RevokeProductBadge;

public class RevokeProductBadgeCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RevokeProductBadgeCommandHandler> logger) : IRequestHandler<RevokeProductBadgeCommand, bool>
{

    public async Task<bool> Handle(RevokeProductBadgeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Revoking product badge. ProductId: {ProductId}, BadgeId: {BadgeId}",
            request.ProductId, request.BadgeId);

        var badge = await context.Set<ProductTrustBadge>()
            .FirstOrDefaultAsync(ptb => ptb.ProductId == request.ProductId && ptb.TrustBadgeId == request.BadgeId, cancellationToken);

        if (badge is null) return false;

        badge.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Product badge revoked successfully. ProductId: {ProductId}, BadgeId: {BadgeId}",
            request.ProductId, request.BadgeId);
        return true;
    }
}

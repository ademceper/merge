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

namespace Merge.Application.Review.Commands.DeleteTrustBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteTrustBadgeCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteTrustBadgeCommandHandler> logger) : IRequestHandler<DeleteTrustBadgeCommand, bool>
{

    public async Task<bool> Handle(DeleteTrustBadgeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting trust badge. BadgeId: {BadgeId}", request.BadgeId);

        var badge = await context.Set<TrustBadge>()
            .FirstOrDefaultAsync(b => b.Id == request.BadgeId, cancellationToken);

        if (badge == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        badge.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Trust badge deleted successfully. BadgeId: {BadgeId}", request.BadgeId);
        return true;
    }
}

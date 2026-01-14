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
public class DeleteTrustBadgeCommandHandler : IRequestHandler<DeleteTrustBadgeCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTrustBadgeCommandHandler> _logger;

    public DeleteTrustBadgeCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteTrustBadgeCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteTrustBadgeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting trust badge. BadgeId: {BadgeId}", request.BadgeId);

        var badge = await _context.Set<TrustBadge>()
            .FirstOrDefaultAsync(b => b.Id == request.BadgeId, cancellationToken);

        if (badge == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        badge.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Trust badge deleted successfully. BadgeId: {BadgeId}", request.BadgeId);
        return true;
    }
}

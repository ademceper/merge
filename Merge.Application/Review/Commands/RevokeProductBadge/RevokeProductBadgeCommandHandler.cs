using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Review.Commands.RevokeProductBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RevokeProductBadgeCommandHandler : IRequestHandler<RevokeProductBadgeCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RevokeProductBadgeCommandHandler> _logger;

    public RevokeProductBadgeCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RevokeProductBadgeCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RevokeProductBadgeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Revoking product badge. ProductId: {ProductId}, BadgeId: {BadgeId}",
            request.ProductId, request.BadgeId);

        var badge = await _context.Set<ProductTrustBadge>()
            .FirstOrDefaultAsync(ptb => ptb.ProductId == request.ProductId && ptb.TrustBadgeId == request.BadgeId, cancellationToken);

        if (badge == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        badge.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Product badge revoked successfully. ProductId: {ProductId}, BadgeId: {BadgeId}",
            request.ProductId, request.BadgeId);
        return true;
    }
}

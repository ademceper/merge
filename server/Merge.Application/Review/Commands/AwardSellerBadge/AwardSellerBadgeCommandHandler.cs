using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.AwardSellerBadge;

public class AwardSellerBadgeCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<AwardSellerBadgeCommandHandler> logger) : IRequestHandler<AwardSellerBadgeCommand, SellerTrustBadgeDto>
{

    public async Task<SellerTrustBadgeDto> Handle(AwardSellerBadgeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Awarding seller badge. SellerId: {SellerId}, BadgeId: {BadgeId}",
            request.SellerId, request.BadgeId);

        var existing = await context.Set<SellerTrustBadge>()
            .FirstOrDefaultAsync(stb => stb.SellerId == request.SellerId && stb.TrustBadgeId == request.BadgeId, cancellationToken);

        if (existing != null)
        {
            existing.Activate();
            existing.UpdateAwardedAt(DateTime.UtcNow);
            existing.UpdateExpiryDate(request.ExpiresAt);
            existing.UpdateAwardReason(request.AwardReason);
        }
        else
        {
            var sellerBadge = SellerTrustBadge.Create(
                request.SellerId,
                request.BadgeId,
                DateTime.UtcNow,
                request.ExpiresAt,
                request.AwardReason);

            await context.Set<SellerTrustBadge>().AddAsync(sellerBadge, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var sellerBadgeDto = await GetSellerBadgeDtoAsync(request.SellerId, request.BadgeId, cancellationToken);
        return sellerBadgeDto;
    }

    private async Task<SellerTrustBadgeDto> GetSellerBadgeDtoAsync(Guid sellerId, Guid badgeId, CancellationToken cancellationToken)
    {
        var sellerBadge = await context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badgeId, cancellationToken);

        if (sellerBadge == null)
            throw new NotFoundException("Satıcı rozeti", badgeId);

        return mapper.Map<SellerTrustBadgeDto>(sellerBadge);
    }
}

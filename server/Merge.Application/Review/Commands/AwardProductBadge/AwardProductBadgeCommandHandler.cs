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

namespace Merge.Application.Review.Commands.AwardProductBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AwardProductBadgeCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<AwardProductBadgeCommandHandler> logger) : IRequestHandler<AwardProductBadgeCommand, ProductTrustBadgeDto>
{

    public async Task<ProductTrustBadgeDto> Handle(AwardProductBadgeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Awarding product badge. ProductId: {ProductId}, BadgeId: {BadgeId}",
            request.ProductId, request.BadgeId);

        var existing = await context.Set<ProductTrustBadge>()
            .FirstOrDefaultAsync(ptb => ptb.ProductId == request.ProductId && ptb.TrustBadgeId == request.BadgeId, cancellationToken);

        if (existing != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            existing.Activate();
            existing.UpdateAwardedAt(DateTime.UtcNow);
            existing.UpdateExpiryDate(request.ExpiresAt);
            existing.UpdateAwardReason(request.AwardReason);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var productBadge = ProductTrustBadge.Create(
                request.ProductId,
                request.BadgeId,
                DateTime.UtcNow,
                request.ExpiresAt,
                request.AwardReason);

            await context.Set<ProductTrustBadge>().AddAsync(productBadge, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var productBadgeDto = await GetProductBadgeDtoAsync(request.ProductId, request.BadgeId, cancellationToken);
        return productBadgeDto;
    }

    private async Task<ProductTrustBadgeDto> GetProductBadgeDtoAsync(Guid productId, Guid badgeId, CancellationToken cancellationToken)
    {
        var productBadge = await context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badgeId, cancellationToken);

        if (productBadge == null)
            throw new NotFoundException("Ürün rozeti", badgeId);

        return mapper.Map<ProductTrustBadgeDto>(productBadge);
    }
}

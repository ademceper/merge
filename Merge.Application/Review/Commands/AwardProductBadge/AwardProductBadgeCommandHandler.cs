using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Review.Commands.AwardProductBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AwardProductBadgeCommandHandler : IRequestHandler<AwardProductBadgeCommand, ProductTrustBadgeDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AwardProductBadgeCommandHandler> _logger;

    public AwardProductBadgeCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AwardProductBadgeCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductTrustBadgeDto> Handle(AwardProductBadgeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Awarding product badge. ProductId: {ProductId}, BadgeId: {BadgeId}",
            request.ProductId, request.BadgeId);

        var existing = await _context.Set<ProductTrustBadge>()
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

            await _context.Set<ProductTrustBadge>().AddAsync(productBadge, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var productBadgeDto = await GetProductBadgeDtoAsync(request.ProductId, request.BadgeId, cancellationToken);
        return productBadgeDto;
    }

    private async Task<ProductTrustBadgeDto> GetProductBadgeDtoAsync(Guid productId, Guid badgeId, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var productBadge = await _context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .FirstOrDefaultAsync(ptb => ptb.ProductId == productId && ptb.TrustBadgeId == badgeId, cancellationToken);

        if (productBadge == null)
            throw new NotFoundException("Ürün rozeti", badgeId);

        return _mapper.Map<ProductTrustBadgeDto>(productBadge);
    }
}

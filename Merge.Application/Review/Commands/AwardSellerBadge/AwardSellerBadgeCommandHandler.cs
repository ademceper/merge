using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Review.Commands.AwardSellerBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AwardSellerBadgeCommandHandler : IRequestHandler<AwardSellerBadgeCommand, SellerTrustBadgeDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AwardSellerBadgeCommandHandler> _logger;

    public AwardSellerBadgeCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AwardSellerBadgeCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SellerTrustBadgeDto> Handle(AwardSellerBadgeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Awarding seller badge. SellerId: {SellerId}, BadgeId: {BadgeId}",
            request.SellerId, request.BadgeId);

        var existing = await _context.Set<SellerTrustBadge>()
            .FirstOrDefaultAsync(stb => stb.SellerId == request.SellerId && stb.TrustBadgeId == request.BadgeId, cancellationToken);

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
            var sellerBadge = SellerTrustBadge.Create(
                request.SellerId,
                request.BadgeId,
                DateTime.UtcNow,
                request.ExpiresAt,
                request.AwardReason);

            await _context.Set<SellerTrustBadge>().AddAsync(sellerBadge, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sellerBadgeDto = await GetSellerBadgeDtoAsync(request.SellerId, request.BadgeId, cancellationToken);
        return sellerBadgeDto;
    }

    private async Task<SellerTrustBadgeDto> GetSellerBadgeDtoAsync(Guid sellerId, Guid badgeId, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var sellerBadge = await _context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .FirstOrDefaultAsync(stb => stb.SellerId == sellerId && stb.TrustBadgeId == badgeId, cancellationToken);

        if (sellerBadge == null)
            throw new NotFoundException("Satıcı rozeti", badgeId);

        return _mapper.Map<SellerTrustBadgeDto>(sellerBadge);
    }
}

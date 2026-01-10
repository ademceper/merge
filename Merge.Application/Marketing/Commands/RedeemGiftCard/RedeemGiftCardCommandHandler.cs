using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Commands.RedeemGiftCard;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RedeemGiftCardCommandHandler : IRequestHandler<RedeemGiftCardCommand, GiftCardDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RedeemGiftCardCommandHandler> _logger;

    public RedeemGiftCardCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<RedeemGiftCardCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GiftCardDto> Handle(RedeemGiftCardCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Redeeming gift card. Code: {Code}, UserId: {UserId}", request.Code, request.UserId);

        var giftCard = await _context.Set<GiftCard>()
            .FirstOrDefaultAsync(gc => gc.Code == request.Code, cancellationToken);

        if (giftCard == null)
        {
            _logger.LogWarning("GiftCard not found. Code: {Code}", request.Code);
            throw new NotFoundException("Hediye kartı", Guid.Empty);
        }

        if (!giftCard.IsActive || giftCard.IsRedeemed)
        {
            throw new BusinessException("Hediye kartı kullanılamaz durumda.");
        }

        if (DateTime.UtcNow > giftCard.ExpiresAt)
        {
            throw new BusinessException("Hediye kartının süresi dolmuş.");
        }

        if (giftCard.RemainingAmount <= 0)
        {
            throw new BusinessException("Hediye kartı bakiyesi yetersiz.");
        }

        // Kullanıcıya atanmış mı kontrol et
        if (giftCard.AssignedToUserId.HasValue && giftCard.AssignedToUserId.Value != request.UserId)
        {
            throw new BusinessException("Bu hediye kartı size atanmamış.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        // Eğer atanmamışsa, kullanıcıya ata
        if (!giftCard.AssignedToUserId.HasValue)
        {
            giftCard.AssignTo(request.UserId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        giftCard.Redeem();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking ile tek query'de getir
        var updatedGiftCard = await _context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == request.Code, cancellationToken);

        if (updatedGiftCard == null)
        {
            _logger.LogWarning("GiftCard not found after redemption. Code: {Code}", request.Code);
            throw new NotFoundException("Hediye kartı", Guid.Empty);
        }

        _logger.LogInformation("GiftCard redeemed successfully. Code: {Code}, UserId: {UserId}", request.Code, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<GiftCardDto>(updatedGiftCard);
    }
}

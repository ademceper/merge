using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.RedeemGiftCard;

public class RedeemGiftCardCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<RedeemGiftCardCommandHandler> logger) : IRequestHandler<RedeemGiftCardCommand, GiftCardDto>
{
    public async Task<GiftCardDto> Handle(RedeemGiftCardCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Redeeming gift card. Code: {Code}, UserId: {UserId}", request.Code, request.UserId);

        var giftCard = await context.Set<GiftCard>()
            .FirstOrDefaultAsync(gc => gc.Code == request.Code, cancellationToken);

        if (giftCard is null)
        {
            logger.LogWarning("GiftCard not found. Code: {Code}", request.Code);
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

        // Eğer atanmamışsa, kullanıcıya ata
        if (!giftCard.AssignedToUserId.HasValue)
        {
            giftCard.AssignTo(request.UserId);
        }

        giftCard.Redeem();

        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedGiftCard = await context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == request.Code, cancellationToken);

        if (updatedGiftCard is null)
        {
            logger.LogWarning("GiftCard not found after redemption. Code: {Code}", request.Code);
            throw new NotFoundException("Hediye kartı", Guid.Empty);
        }

        logger.LogInformation("GiftCard redeemed successfully. Code: {Code}, UserId: {UserId}", request.Code, request.UserId);

        return mapper.Map<GiftCardDto>(updatedGiftCard);
    }
}

using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.PurchaseGiftCard;

public class PurchaseGiftCardCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PurchaseGiftCardCommandHandler> logger,
    IOptions<MarketingSettings> marketingSettings) : IRequestHandler<PurchaseGiftCardCommand, GiftCardDto>
{
    public async Task<GiftCardDto> Handle(PurchaseGiftCardCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Purchasing gift card. UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);

        if (request.Amount <= 0)
        {
            throw new ValidationException("Hediye kartı tutarı 0'dan büyük olmalıdır.");
        }

        var code = await GenerateGiftCardCodeAsync(cancellationToken);
        var amount = new Money(request.Amount);
        var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddYears(marketingSettings.Value.GiftCardExpiryYears);
        
        var giftCard = GiftCard.Create(
            code,
            amount,
            expiresAt,
            request.UserId,
            request.AssignedToUserId,
            request.Message);

        await context.Set<GiftCard>().AddAsync(giftCard, cancellationToken);

        // Transaction kaydı
        var transaction = GiftCardTransaction.Create(
            giftCard.Id,
            amount,
            GiftCardTransactionType.Purchase,
            null,
            "Hediye kartı satın alındı");
        await context.Set<GiftCardTransaction>().AddAsync(transaction, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdGiftCard = await context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Id == giftCard.Id, cancellationToken);

        if (createdGiftCard is null)
        {
            logger.LogWarning("GiftCard not found after creation. GiftCardId: {GiftCardId}", giftCard.Id);
            throw new NotFoundException("Hediye kartı", giftCard.Id);
        }

        logger.LogInformation("GiftCard purchased successfully. GiftCardId: {GiftCardId}, Code: {Code}, UserId: {UserId}", 
            giftCard.Id, code, request.UserId);

        return mapper.Map<GiftCardDto>(createdGiftCard);
    }

    private async Task<string> GenerateGiftCardCodeAsync(CancellationToken cancellationToken)
    {
        // Benzersiz kod oluştur (örn: MERGE-XXXX-XXXX)
        var random = Random.Shared;
        var part1 = random.Next(marketingSettings.Value.GiftCardCodeMinRandom, marketingSettings.Value.GiftCardCodeMaxRandom).ToString();
        var part2 = random.Next(marketingSettings.Value.GiftCardCodeMinRandom, marketingSettings.Value.GiftCardCodeMaxRandom).ToString();
        var code = $"MERGE-{part1}-{part2}";

        while (await context.Set<GiftCard>().AnyAsync(gc => gc.Code == code, cancellationToken))
        {
            part1 = random.Next(marketingSettings.Value.GiftCardCodeMinRandom, marketingSettings.Value.GiftCardCodeMaxRandom).ToString();
            part2 = random.Next(marketingSettings.Value.GiftCardCodeMinRandom, marketingSettings.Value.GiftCardCodeMaxRandom).ToString();
            code = $"MERGE-{part1}-{part2}";
        }

        return code;
    }
}

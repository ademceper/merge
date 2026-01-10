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

namespace Merge.Application.Marketing.Commands.PurchaseGiftCard;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class PurchaseGiftCardCommandHandler : IRequestHandler<PurchaseGiftCardCommand, GiftCardDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PurchaseGiftCardCommandHandler> _logger;
    private readonly MarketingSettings _marketingSettings;

    public PurchaseGiftCardCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PurchaseGiftCardCommandHandler> logger,
        IOptions<MarketingSettings> marketingSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _marketingSettings = marketingSettings.Value;
    }

    public async Task<GiftCardDto> Handle(PurchaseGiftCardCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Purchasing gift card. UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);

        if (request.Amount <= 0)
        {
            throw new ValidationException("Hediye kartı tutarı 0'dan büyük olmalıdır.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
        var code = await GenerateGiftCardCodeAsync(cancellationToken);
        var amount = new Money(request.Amount);
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddYears(_marketingSettings.GiftCardExpiryYears);
        
        var giftCard = GiftCard.Create(
            code,
            amount,
            expiresAt,
            request.UserId,
            request.AssignedToUserId,
            request.Message);

        await _context.Set<GiftCard>().AddAsync(giftCard, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // Transaction kaydı
        var transaction = GiftCardTransaction.Create(
            giftCard.Id,
            amount,
            GiftCardTransactionType.Purchase,
            null,
            "Hediye kartı satın alındı");
        await _context.Set<GiftCardTransaction>().AddAsync(transaction, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking ile tek query'de getir
        var createdGiftCard = await _context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Id == giftCard.Id, cancellationToken);

        if (createdGiftCard == null)
        {
            _logger.LogWarning("GiftCard not found after creation. GiftCardId: {GiftCardId}", giftCard.Id);
            throw new NotFoundException("Hediye kartı", giftCard.Id);
        }

        _logger.LogInformation("GiftCard purchased successfully. GiftCardId: {GiftCardId}, Code: {Code}, UserId: {UserId}", 
            giftCard.Id, code, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<GiftCardDto>(createdGiftCard);
    }

    private async Task<string> GenerateGiftCardCodeAsync(CancellationToken cancellationToken)
    {
        // Benzersiz kod oluştur (örn: MERGE-XXXX-XXXX)
        // ✅ THREAD SAFETY: Random.Shared kullan (new Random() thread-safe değil)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var random = Random.Shared;
        var part1 = random.Next(_marketingSettings.GiftCardCodeMinRandom, _marketingSettings.GiftCardCodeMaxRandom).ToString();
        var part2 = random.Next(_marketingSettings.GiftCardCodeMinRandom, _marketingSettings.GiftCardCodeMaxRandom).ToString();
        var code = $"MERGE-{part1}-{part2}";

        // ✅ PERFORMANCE: AnyAsync kullan (async)
        while (await _context.Set<GiftCard>().AnyAsync(gc => gc.Code == code, cancellationToken))
        {
            part1 = random.Next(_marketingSettings.GiftCardCodeMinRandom, _marketingSettings.GiftCardCodeMaxRandom).ToString();
            part2 = random.Next(_marketingSettings.GiftCardCodeMinRandom, _marketingSettings.GiftCardCodeMaxRandom).ToString();
            code = $"MERGE-{part1}-{part2}";
        }

        return code;
    }
}

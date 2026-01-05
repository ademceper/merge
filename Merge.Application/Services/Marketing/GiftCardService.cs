using AutoMapper;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Entities.Order;
using Merge.Domain.ValueObjects;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;


namespace Merge.Application.Services.Marketing;

public class GiftCardService : IGiftCardService
{
    private readonly IRepository<GiftCard> _giftCardRepository;
    private readonly IRepository<GiftCardTransaction> _transactionRepository;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GiftCardService> _logger;

    public GiftCardService(
        IRepository<GiftCard> giftCardRepository,
        IRepository<GiftCardTransaction> transactionRepository,
        IDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<GiftCardService> logger)
    {
        _giftCardRepository = giftCardRepository;
        _transactionRepository = transactionRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<GiftCardDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        var giftCard = await _context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == code, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return giftCard == null ? null : _mapper.Map<GiftCardDto>(giftCard);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<GiftCardDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Repository yerine direct context kullan (FindAsync yerine FirstOrDefaultAsync)
        var giftCard = await _context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Id == id, cancellationToken);
        
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return giftCard == null ? null : _mapper.Map<GiftCardDto>(giftCard);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<GiftCardDto>> GetUserGiftCardsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        var giftCards = await _context.Set<GiftCard>()
            .AsNoTracking()
            .Where(gc => gc.PurchasedByUserId == userId || gc.AssignedToUserId == userId)
            .OrderByDescending(gc => gc.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<GiftCardDto>>(giftCards);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<GiftCardDto>> GetUserGiftCardsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<GiftCard>()
            .AsNoTracking()
            .Where(gc => gc.PurchasedByUserId == userId || gc.AssignedToUserId == userId)
            .OrderByDescending(gc => gc.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var giftCards = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<GiftCardDto>
        {
            Items = _mapper.Map<List<GiftCardDto>>(giftCards),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<GiftCardDto> PurchaseGiftCardAsync(Guid userId, PurchaseGiftCardDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Hediye kartı satın alma işlemi başlatılıyor. UserId: {UserId}, Amount: {Amount}",
            userId, dto.Amount);

        if (dto.Amount <= 0)
        {
            throw new ValidationException("Hediye kartı tutarı 0'dan büyük olmalıdır.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
        var code = await GenerateGiftCardCodeAsync(cancellationToken);
        var amount = new Money(dto.Amount);
        var expiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddYears(1);
        
        var giftCard = GiftCard.Create(
            code,
            amount,
            expiresAt,
            userId,
            dto.AssignedToUserId,
            dto.Message
        );

        giftCard = await _giftCardRepository.AddAsync(giftCard);

        // Transaction kaydı
        var transaction = new GiftCardTransaction
        {
            GiftCardId = giftCard.Id,
            Amount = dto.Amount,
            TransactionType = "Purchase",
            Notes = "Hediye kartı satın alındı"
        };
        await _transactionRepository.AddAsync(transaction);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        var createdGiftCard = await _context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Id == giftCard.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Hediye kartı satın alma işlemi tamamlandı. GiftCardId: {GiftCardId}, Code: {Code}, UserId: {UserId}",
            giftCard.Id, code, userId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<GiftCardDto>(createdGiftCard!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<GiftCardDto> RedeemGiftCardAsync(string code, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !gc.IsDeleted (Global Query Filter)
        var giftCard = await _context.Set<GiftCard>()
            .FirstOrDefaultAsync(gc => gc.Code == code, cancellationToken);

        if (giftCard == null)
        {
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
        if (giftCard.AssignedToUserId.HasValue && giftCard.AssignedToUserId.Value != userId)
        {
            throw new BusinessException("Bu hediye kartı size atanmamış.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        // Eğer atanmamışsa, kullanıcıya ata
        if (!giftCard.AssignedToUserId.HasValue)
        {
            giftCard.AssignTo(userId);
        }

        await _giftCardRepository.UpdateAsync(giftCard);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        var updatedGiftCard = await _context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == code, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<GiftCardDto>(updatedGiftCard!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default)
    {
        var giftCard = await GetByCodeAsync(code, cancellationToken);
        if (giftCard == null || !giftCard.IsValid)
        {
            return 0;
        }

        // Hediye kartı bakiyesi kadar indirim uygula
        return Math.Min(giftCard.RemainingAmount, orderAmount);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ApplyGiftCardToOrderAsync(string code, Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !gc.IsDeleted (Global Query Filter)
        var giftCard = await _context.Set<GiftCard>()
            .FirstOrDefaultAsync(gc => gc.Code == code, cancellationToken);

        if (giftCard == null || !giftCard.IsActive || giftCard.RemainingAmount <= 0)
        {
            return false;
        }

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);

        if (order == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        var discountAmount = Math.Min(giftCard.RemainingAmount, order.TotalAmount);
        var discountMoney = new Money(discountAmount);
        
        giftCard.Use(discountMoney);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        order.ApplyGiftCardDiscount(discountMoney);

        await _giftCardRepository.UpdateAsync(giftCard);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Transaction kaydı
        var transaction = new GiftCardTransaction
        {
            GiftCardId = giftCard.Id,
            OrderId = orderId,
            Amount = discountAmount,
            TransactionType = "Redeem",
            Notes = $"Sipariş #{order.OrderNumber} için kullanıldı"
        };
        await _transactionRepository.AddAsync(transaction);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<string> GenerateGiftCardCodeAsync(CancellationToken cancellationToken = default)
    {
        // Benzersiz kod oluştur (örn: MERGE-XXXX-XXXX)
        // ✅ THREAD SAFETY: Random.Shared kullan (new Random() thread-safe değil)
        var random = Random.Shared;
        var part1 = random.Next(1000, 9999).ToString();
        var part2 = random.Next(1000, 9999).ToString();
        var code = $"MERGE-{part1}-{part2}";

        // ✅ PERFORMANCE: Removed manual !gc.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AnyAsync kullan (async)
        while (await _context.Set<GiftCard>().AnyAsync(gc => gc.Code == code, cancellationToken))
        {
            part1 = random.Next(1000, 9999).ToString();
            part2 = random.Next(1000, 9999).ToString();
            code = $"MERGE-{part1}-{part2}";
        }

        return code;
    }
}


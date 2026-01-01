using AutoMapper;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Marketing;


namespace Merge.Application.Services.Marketing;

public class GiftCardService : IGiftCardService
{
    private readonly IRepository<GiftCard> _giftCardRepository;
    private readonly IRepository<GiftCardTransaction> _transactionRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GiftCardService> _logger;

    public GiftCardService(
        IRepository<GiftCard> giftCardRepository,
        IRepository<GiftCardTransaction> transactionRepository,
        ApplicationDbContext context,
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

    public async Task<GiftCardDto?> GetByCodeAsync(string code)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        var giftCard = await _context.GiftCards
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return giftCard == null ? null : _mapper.Map<GiftCardDto>(giftCard);
    }

    public async Task<GiftCardDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Repository yerine direct context kullan (FindAsync yerine FirstOrDefaultAsync)
        var giftCard = await _context.GiftCards
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Id == id);
        
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return giftCard == null ? null : _mapper.Map<GiftCardDto>(giftCard);
    }

    public async Task<IEnumerable<GiftCardDto>> GetUserGiftCardsAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        var giftCards = await _context.GiftCards
            .AsNoTracking()
            .Where(gc => gc.PurchasedByUserId == userId || gc.AssignedToUserId == userId)
            .OrderByDescending(gc => gc.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<GiftCardDto>>(giftCards);
    }

    public async Task<GiftCardDto> PurchaseGiftCardAsync(Guid userId, PurchaseGiftCardDto dto)
    {
        if (dto.Amount <= 0)
        {
            throw new ValidationException("Hediye kartı tutarı 0'dan büyük olmalıdır.");
        }

        var code = await GenerateGiftCardCodeAsync();

        var giftCard = new GiftCard
        {
            Code = code,
            Amount = dto.Amount,
            RemainingAmount = dto.Amount,
            PurchasedByUserId = userId,
            AssignedToUserId = dto.AssignedToUserId,
            Message = dto.Message,
            ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddYears(1),
            IsActive = true,
            IsRedeemed = false
        };

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
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        var createdGiftCard = await _context.GiftCards
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Id == giftCard.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<GiftCardDto>(createdGiftCard!);
    }

    public async Task<GiftCardDto> RedeemGiftCardAsync(string code, Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !gc.IsDeleted (Global Query Filter)
        var giftCard = await _context.GiftCards
            .FirstOrDefaultAsync(gc => gc.Code == code);

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

        // Eğer atanmamışsa, kullanıcıya ata
        if (!giftCard.AssignedToUserId.HasValue)
        {
            giftCard.AssignedToUserId = userId;
        }

        await _giftCardRepository.UpdateAsync(giftCard);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !gc.IsDeleted (Global Query Filter)
        var updatedGiftCard = await _context.GiftCards
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<GiftCardDto>(updatedGiftCard!);
    }

    public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount)
    {
        var giftCard = await GetByCodeAsync(code);
        if (giftCard == null || !giftCard.IsValid)
        {
            return 0;
        }

        // Hediye kartı bakiyesi kadar indirim uygula
        return Math.Min(giftCard.RemainingAmount, orderAmount);
    }

    public async Task<bool> ApplyGiftCardToOrderAsync(string code, Guid orderId, Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !gc.IsDeleted (Global Query Filter)
        var giftCard = await _context.GiftCards
            .FirstOrDefaultAsync(gc => gc.Code == code);

        if (giftCard == null || !giftCard.IsActive || giftCard.RemainingAmount <= 0)
        {
            return false;
        }

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            return false;
        }

        var discountAmount = Math.Min(giftCard.RemainingAmount, order.TotalAmount);
        
        giftCard.RemainingAmount -= discountAmount;
        if (giftCard.RemainingAmount <= 0)
        {
            giftCard.IsRedeemed = true;
            giftCard.RedeemedAt = DateTime.UtcNow;
        }

        order.GiftCardDiscount = discountAmount;
        order.TotalAmount -= discountAmount;

        await _giftCardRepository.UpdateAsync(giftCard);
        await _unitOfWork.SaveChangesAsync();

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

    private async Task<string> GenerateGiftCardCodeAsync()
    {
        // Benzersiz kod oluştur (örn: MERGE-XXXX-XXXX)
        var random = new Random();
        var part1 = random.Next(1000, 9999).ToString();
        var part2 = random.Next(1000, 9999).ToString();
        var code = $"MERGE-{part1}-{part2}";

        // ✅ PERFORMANCE: Removed manual !gc.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AnyAsync kullan (async)
        while (await _context.GiftCards.AnyAsync(gc => gc.Code == code))
        {
            part1 = random.Next(1000, 9999).ToString();
            part2 = random.Next(1000, 9999).ToString();
            code = $"MERGE-{part1}-{part2}";
        }

        return code;
    }
}


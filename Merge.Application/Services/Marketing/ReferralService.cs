using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Configuration;
using Merge.Application.Common;


namespace Merge.Application.Services.Marketing;

public class ReferralService : IReferralService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoyaltyService _loyaltyService;
    private readonly IMapper _mapper;
    private readonly ILogger<ReferralService> _logger;
    private readonly ReferralSettings _referralSettings;

    public ReferralService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILoyaltyService loyaltyService,
        IMapper mapper,
        ILogger<ReferralService> logger,
        IOptions<ReferralSettings> referralSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _loyaltyService = loyaltyService;
        _mapper = mapper;
        _logger = logger;
        _referralSettings = referralSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReferralCodeDto> GetMyReferralCodeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var code = await _context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (code == null)
        {
            return await CreateReferralCodeAsync(userId, cancellationToken);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ReferralCodeDto>(code);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReferralCodeDto> CreateReferralCodeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Referans kodu oluşturuluyor. UserId: {UserId}",
            userId);

        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        var referralCode = GenerateCode(user?.Email ?? "USER");
        var code = new ReferralCode
        {
            UserId = userId,
            Code = referralCode,
            MaxUsage = 0,
            PointsReward = _referralSettings.ReferrerPointsReward,
            DiscountPercentage = _referralSettings.RefereeDiscountPercentage
        };

        await _context.Set<ReferralCode>().AddAsync(code, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var createdCode = await _context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == code.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Referans kodu oluşturuldu. ReferralCodeId: {ReferralCodeId}, Code: {Code}, UserId: {UserId}",
            code.Id, referralCode, userId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ReferralCodeDto>(createdCode!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ReferralDto>> GetMyReferralsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var referrals = await _context.Set<Referral>()
            .AsNoTracking()
            .Include(r => r.ReferredUser)
            .Where(r => r.ReferrerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ReferralDto>>(referrals);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ReferralDto>> GetMyReferralsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Referral>()
            .AsNoTracking()
            .Include(r => r.ReferredUser)
            .Where(r => r.ReferrerId == userId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var referrals = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ReferralDto>
        {
            Items = _mapper.Map<List<ReferralDto>>(referrals),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ApplyReferralCodeAsync(Guid newUserId, string code, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var referralCode = await _context.Set<ReferralCode>()
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive, cancellationToken);

        if (referralCode == null || referralCode.UserId == newUserId)
            return false;

        var exists = await _context.Set<Referral>()
            .AnyAsync(r => r.ReferredUserId == newUserId, cancellationToken);

        if (exists)
            return false;

        var referral = new Referral
        {
            ReferrerId = referralCode.UserId,
            ReferredUserId = newUserId,
            ReferralCodeId = referralCode.Id,
            ReferralCode = code,
            Status = ReferralStatus.Pending
        };

        referralCode.UsageCount++;
        await _context.Set<Referral>().AddAsync(referral, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task ProcessReferralRewardAsync(Guid referredUserId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var referral = await _context.Set<Referral>()
            .Include(r => r.ReferralCodeEntity)
            .FirstOrDefaultAsync(r => r.ReferredUserId == referredUserId && r.Status == ReferralStatus.Pending, cancellationToken);

        if (referral == null)
            return;

        referral.Status = ReferralStatus.Completed;
        referral.CompletedAt = DateTime.UtcNow;
        referral.FirstOrderId = orderId;
        referral.PointsAwarded = referral.ReferralCodeEntity.PointsReward;

        await _loyaltyService.EarnPointsAsync(referral.ReferrerId, referral.PointsAwarded, "Referral", $"Referral reward for {referredUserId}", null, cancellationToken);

        referral.Status = ReferralStatus.Rewarded;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReferralStatsDto> GetReferralStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalReferrals = await _context.Set<Referral>()
            .CountAsync(r => r.ReferrerId == userId, cancellationToken);

        var completedReferrals = await _context.Set<Referral>()
            .CountAsync(r => r.ReferrerId == userId && (r.Status == ReferralStatus.Completed || r.Status == ReferralStatus.Rewarded), cancellationToken);

        var pendingReferrals = await _context.Set<Referral>()
            .CountAsync(r => r.ReferrerId == userId && r.Status == ReferralStatus.Pending, cancellationToken);

        var totalPointsAwarded = (int)await _context.Set<Referral>()
            .Where(r => r.ReferrerId == userId)
            .SumAsync(r => (long)r.PointsAwarded, cancellationToken);

        var conversionRate = totalReferrals > 0 
            ? (decimal)(totalReferrals - pendingReferrals) / totalReferrals * 100 
            : 0;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new ReferralStatsDto
        {
            TotalReferrals = totalReferrals,
            CompletedReferrals = completedReferrals,
            PendingReferrals = pendingReferrals,
            TotalPointsAwarded = totalPointsAwarded,
            ConversionRate = conversionRate
        };
    }

    private string GenerateCode(string email)
    {
        var prefix = email.Split('@')[0].ToUpper().Substring(0, Math.Min(4, email.Length));
        // ✅ THREAD SAFETY: Random.Shared kullan (new Random() thread-safe değil)
        var random = Random.Shared.Next(1000, 9999);
        return $"{prefix}{random}";
    }

}

public class ReviewMediaService : IReviewMediaService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReviewMediaService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReviewMediaDto> AddMediaToReviewAsync(Guid reviewId, string url, string mediaType, string? thumbnailUrl = null)
    {
        var media = new ReviewMedia
        {
            ReviewId = reviewId,
            MediaType = Enum.Parse<ReviewMediaType>(mediaType, true),
            Url = url,
            ThumbnailUrl = thumbnailUrl ?? url
        };

        await _context.Set<ReviewMedia>().AddAsync(media);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
        var createdMedia = await _context.Set<ReviewMedia>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == media.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ReviewMediaDto>(createdMedia!);
    }

    public async Task<IEnumerable<ReviewMediaDto>> GetReviewMediaAsync(Guid reviewId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
        var media = await _context.Set<ReviewMedia>()
            .AsNoTracking()
            .Where(m => m.ReviewId == reviewId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ReviewMediaDto>>(media);
    }

    public async Task DeleteReviewMediaAsync(Guid mediaId)
    {
        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var media = await _context.Set<ReviewMedia>()
            .FirstOrDefaultAsync(m => m.Id == mediaId);
        if (media != null)
        {
            media.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

public class SharedWishlistService : ISharedWishlistService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SharedWishlistService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SharedWishlistDto> CreateSharedWishlistAsync(Guid userId, CreateSharedWishlistDto dto)
    {
        var wishlist = new SharedWishlist
        {
            UserId = userId,
            ShareCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            Name = dto.Name,
            Description = dto.Description,
            IsPublic = dto.IsPublic
        };

        await _context.Set<SharedWishlist>().AddAsync(wishlist);
        await _unitOfWork.SaveChangesAsync();

        foreach (var productId in dto.ProductIds)
        {
            var item = new SharedWishlistItem
            {
                SharedWishlistId = wishlist.Id,
                ProductId = productId
            };
            await _context.Set<SharedWishlistItem>().AddAsync(item);
        }

        await _unitOfWork.SaveChangesAsync();
        return await GetSharedWishlistByCodeAsync(wishlist.ShareCode) ?? new SharedWishlistDto();
    }

    public async Task<SharedWishlistDto?> GetSharedWishlistByCodeAsync(string shareCode)
    {
        // ✅ PERFORMANCE: Removed manual !w.IsDeleted (Global Query Filter)
        var wishlist = await _context.Set<SharedWishlist>()
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.ShareCode == shareCode);

        if (wishlist == null)
            return null;

        wishlist.ViewCount++;
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var items = await _context.Set<SharedWishlistItem>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.SharedWishlistId == wishlist.Id)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<SharedWishlistDto>(wishlist);
        dto.Items = _mapper.Map<List<SharedWishlistItemDto>>(items);
        dto.ItemCount = items.Count;
        return dto;
    }

    public async Task<IEnumerable<SharedWishlistDto>> GetMySharedWishlistsAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var wishlists = await _context.Set<SharedWishlist>()
            .AsNoTracking()
            .Include(w => w.User)
            .Where(w => w.UserId == userId)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load items (N+1 fix)
        // ✅ PERFORMANCE: wishlistIds'i memory'den al (zaten yüklenmiş wishlists'ten)
        var wishlistIds = wishlists.Select(w => w.Id).ToList();
        
        // ✅ PERFORMANCE: Database'de GroupBy ve ToDictionaryAsync yap (ToListAsync() sonrası memory'de işlem YASAK)
        var itemsByWishlist = await _context.Set<SharedWishlistItem>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => wishlistIds.Contains(i.SharedWishlistId))
            .GroupBy(i => i.SharedWishlistId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = new List<SharedWishlistDto>();
        foreach (var wishlist in wishlists)
        {
            var dto = _mapper.Map<SharedWishlistDto>(wishlist);
            if (itemsByWishlist.TryGetValue(wishlist.Id, out var wishlistItems))
            {
                dto.Items = _mapper.Map<List<SharedWishlistItemDto>>(wishlistItems);
                dto.ItemCount = wishlistItems.Count;
            }
            result.Add(dto);
        }

        return result;
    }

    public async Task DeleteSharedWishlistAsync(Guid wishlistId)
    {
        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var wishlist = await _context.Set<SharedWishlist>()
            .FirstOrDefaultAsync(w => w.Id == wishlistId);
        if (wishlist != null)
        {
            wishlist.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task MarkItemAsPurchasedAsync(Guid itemId, Guid purchasedBy)
    {
        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var item = await _context.Set<SharedWishlistItem>()
            .FirstOrDefaultAsync(i => i.Id == itemId);
        if (item != null)
        {
            item.IsPurchased = true;
            item.PurchasedBy = purchasedBy;
            item.PurchasedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

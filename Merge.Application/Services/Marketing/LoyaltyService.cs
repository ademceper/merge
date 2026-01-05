using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Configuration;
using Merge.Application.Common;

namespace Merge.Application.Services.Marketing;

public class LoyaltyService : ILoyaltyService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LoyaltyService> _logger;
    private readonly LoyaltySettings _loyaltySettings;
    private const decimal POINTS_TO_CURRENCY_RATE = 0.01m; // 1 point = $0.01
    private const decimal CURRENCY_TO_POINTS_RATE = 1.0m; // $1 = 1 point

    public LoyaltyService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<LoyaltyService> logger,
        IOptions<LoyaltySettings> loyaltySettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _loyaltySettings = loyaltySettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LoyaltyAccountDto?> GetLoyaltyAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var account = await _context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .Include(a => a.Tier)
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return account != null ? _mapper.Map<LoyaltyAccountDto>(account) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LoyaltyAccountDto> CreateLoyaltyAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Sadakat hesabı oluşturuluyor. UserId: {UserId}",
            userId);

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var exists = await _context.Set<LoyaltyAccount>()
            .AnyAsync(a => a.UserId == userId, cancellationToken);

        if (exists)
        {
            throw new BusinessException("Sadakat hesabı zaten mevcut.");
        }

        var account = new LoyaltyAccount
        {
            UserId = userId,
            PointsBalance = 0,
            LifetimePoints = 0
        };

        await _context.Set<LoyaltyAccount>().AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        await EarnPointsAsync(userId, _loyaltySettings.SignupBonusPoints, "Signup", "Welcome bonus", null, cancellationToken);

        var created = await GetLoyaltyAccountAsync(userId, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Sadakat hesabı oluşturuldu. LoyaltyAccountId: {LoyaltyAccountId}, UserId: {UserId}, SignupBonusPoints: {SignupBonusPoints}",
            account.Id, userId, _loyaltySettings.SignupBonusPoints);

        return created!;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<LoyaltyTransactionDto>> GetTransactionsAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var transactions = await _context.Set<LoyaltyTransaction>()
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.CreatedAt >= startDate)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<LoyaltyTransactionDto>>(transactions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<LoyaltyTransactionDto>> GetTransactionsAsync(Guid userId, int days, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var query = _context.Set<LoyaltyTransaction>()
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.CreatedAt >= startDate)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LoyaltyTransactionDto>
        {
            Items = _mapper.Map<List<LoyaltyTransactionDto>>(transactions),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task EarnPointsAsync(Guid userId, int points, string type, string description, Guid? orderId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var account = await _context.Set<LoyaltyAccount>()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (account == null)
        {
            account = new LoyaltyAccount { UserId = userId };
            await _context.Set<LoyaltyAccount>().AddAsync(account, cancellationToken);
        }

        // Apply tier multiplier
        var multiplier = account.Tier?.PointsMultiplier ?? 1.0m;
        var adjustedPoints = (int)(points * multiplier);

        account.PointsBalance += adjustedPoints;
        account.LifetimePoints += adjustedPoints;

        var transaction = new LoyaltyTransaction
        {
            UserId = userId,
            LoyaltyAccountId = account.Id,
            Points = adjustedPoints,
            Type = Enum.Parse<LoyaltyTransactionType>(type, true),
            Description = description,
            OrderId = orderId,
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        };

        await _context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);

        // Check tier upgrade
        await UpdateTierAsync(account, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> RedeemPointsAsync(Guid userId, int points, Guid? orderId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var account = await _context.Set<LoyaltyAccount>()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (account == null || account.PointsBalance < points)
        {
            return false;
        }

        account.PointsBalance -= points;

        var transaction = new LoyaltyTransaction
        {
            UserId = userId,
            LoyaltyAccountId = account.Id,
            Points = -points,
            Type = LoyaltyTransactionType.Redeem,
            Description = $"Redeemed {points} points",
            OrderId = orderId,
            ExpiresAt = DateTime.UtcNow
        };

        await _context.Set<LoyaltyTransaction>().AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public Task<int> CalculatePointsFromPurchaseAsync(decimal amount, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((int)(amount * CURRENCY_TO_POINTS_RATE));
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public Task<decimal> CalculateDiscountFromPointsAsync(int points, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(points * POINTS_TO_CURRENCY_RATE);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<LoyaltyTierDto>> GetTiersAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var tiers = await _context.Set<LoyaltyTier>()
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Level)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<LoyaltyTierDto>>(tiers);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LoyaltyStatsDto> GetLoyaltyStatsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var totalMembers = await _context.Set<LoyaltyAccount>()
            .CountAsync(cancellationToken);

        var totalPointsIssued = await _context.Set<LoyaltyAccount>()
            .SumAsync(a => (long)a.LifetimePoints, cancellationToken);

        var totalPointsRedeemed = await _context.Set<LoyaltyTransaction>()
            .Where(t => t.Points < 0)
            .SumAsync(t => (long)Math.Abs(t.Points), cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var membersByTier = await _context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .Include(a => a.Tier)
            .GroupBy(a => a.Tier != null ? a.Tier.Name : "No Tier")
            .Select(g => new { Tier = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Tier, x => x.Count, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new LoyaltyStatsDto
        {
            TotalMembers = totalMembers,
            TotalPointsIssued = totalPointsIssued,
            TotalPointsRedeemed = totalPointsRedeemed,
            MembersByTier = membersByTier,
            AveragePointsPerMember = totalMembers > 0 ? (decimal)totalPointsIssued / totalMembers : 0
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task ExpirePointsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Batch load accounts (N+1 fix)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() ve Distinct() YASAK - Database'de Select ve Distinct yap
        var accountIds = await _context.Set<LoyaltyTransaction>()
            .AsNoTracking()
            .Where(t => !t.IsExpired && t.ExpiresAt < now && t.Points > 0)
            .Select(t => t.LoyaltyAccountId)
            .Distinct()
            .ToListAsync(cancellationToken);
        
        if (accountIds.Count == 0)
        {
            return;
        }
        
        var accounts = await _context.Set<LoyaltyAccount>()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);
        
        // ✅ PERFORMANCE: Reload expired transactions for update (tracking gerekli)
        var expiredTransactions = await _context.Set<LoyaltyTransaction>()
            .Where(t => !t.IsExpired && t.ExpiresAt < now && t.Points > 0)
            .ToListAsync(cancellationToken);

        foreach (var transaction in expiredTransactions)
        {
            transaction.IsExpired = true;

            if (accounts.TryGetValue(transaction.LoyaltyAccountId, out var account))
            {
                account.PointsBalance -= transaction.Points;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task UpdateTierAsync(LoyaltyAccount account, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de filtreleme yap (memory'de işlem YASAK)
        var newTier = await _context.Set<LoyaltyTier>()
            .Where(t => t.IsActive && account.LifetimePoints >= t.MinimumPoints)
            .OrderByDescending(t => t.MinimumPoints)
            .FirstOrDefaultAsync(cancellationToken);

        if (newTier != null && account.TierId != newTier.Id)
        {
            account.TierId = newTier.Id;
            account.TierAchievedAt = DateTime.UtcNow;
            account.TierExpiresAt = DateTime.UtcNow.AddYears(1);
        }
    }
}

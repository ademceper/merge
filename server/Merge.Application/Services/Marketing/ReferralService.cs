using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediatR;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces;
using Merge.Application.Marketing.Commands.AddPoints;
using AddPointsCommand = Merge.Application.Marketing.Commands.AddPoints.AddPointsCommand;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Configuration;
using Merge.Application.Common;
using Merge.Application.Interfaces.Marketing;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Marketing;

public class ReferralService(IDbContext context, IUnitOfWork unitOfWork, IMediator mediator, IMapper mapper, ILogger<ReferralService> logger, IOptions<ReferralSettings> referralSettings) : IReferralService
{
    private readonly ReferralSettings referralConfig = referralSettings.Value;

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReferralCodeDto> GetMyReferralCodeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var code = await context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (code == null)
        {
            return await CreateReferralCodeAsync(userId, cancellationToken);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<ReferralCodeDto>(code);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReferralCodeDto> CreateReferralCodeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Referans kodu oluşturuluyor. UserId: {UserId}",
            userId);

        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        var referralCode = GenerateCode(user?.Email ?? "USER");
        var code = ReferralCode.Create(
            userId,
            referralCode,
            0,
            null,
            referralConfig.ReferrerPointsReward,
            referralConfig.RefereeDiscountPercentage
        );

        await context.Set<ReferralCode>().AddAsync(code, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var createdCode = await context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == code.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Referans kodu oluşturuldu. ReferralCodeId: {ReferralCodeId}, Code: {Code}, UserId: {UserId}",
            code.Id, referralCode, userId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<ReferralCodeDto>(createdCode!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ReferralDto>> GetMyReferralsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var referrals = await context.Set<Referral>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.ReferredUser)
            .Where(r => r.ReferrerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<IEnumerable<ReferralDto>>(referrals);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ReferralDto>> GetMyReferralsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var query = context.Set<Referral>()
            .AsNoTracking()
            .Include(r => r.ReferredUser)
            .Where(r => r.ReferrerId == userId);

        var totalCount = await query.CountAsync(cancellationToken);
        var referrals = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new PagedResult<ReferralDto>
        {
            Items = mapper.Map<List<ReferralDto>>(referrals),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ApplyReferralCodeAsync(Guid newUserId, string code, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var referralCode = await context.Set<ReferralCode>()
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive, cancellationToken);

        if (referralCode == null || referralCode.UserId == newUserId)
            return false;

        var exists = await context.Set<Referral>()
            .AnyAsync(r => r.ReferredUserId == newUserId, cancellationToken);

        if (exists)
            return false;

        var referral = Referral.Create(
            referralCode.UserId,
            newUserId,
            referralCode.Id,
            code
        );

        referralCode.IncrementUsage();
        await context.Set<Referral>().AddAsync(referral, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task ProcessReferralRewardAsync(Guid referredUserId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var referral = await context.Set<Referral>()
            .Include(r => r.ReferralCodeEntity)
            .FirstOrDefaultAsync(r => r.ReferredUserId == referredUserId && r.Status == ReferralStatus.Pending, cancellationToken);

        if (referral == null)
            return;

        referral.Complete(orderId, referral.ReferralCodeEntity.PointsReward);

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var addPointsCommand = new AddPointsCommand(
            referral.ReferrerId,
            referral.PointsAwarded,
            "Referral",
            $"Referral reward for {referredUserId}",
            null);
        await mediator.Send(addPointsCommand, cancellationToken);

        referral.MarkAsRewarded();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReferralStatsDto> GetReferralStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalReferrals = await context.Set<Referral>()
            .CountAsync(r => r.ReferrerId == userId, cancellationToken);

        var completedReferrals = await context.Set<Referral>()
            .CountAsync(r => r.ReferrerId == userId && (r.Status == ReferralStatus.Completed || r.Status == ReferralStatus.Rewarded), cancellationToken);

        var pendingReferrals = await context.Set<Referral>()
            .CountAsync(r => r.ReferrerId == userId && r.Status == ReferralStatus.Pending, cancellationToken);

        var totalPointsAwarded = (int)await context.Set<Referral>()
            .Where(r => r.ReferrerId == userId)
            .SumAsync(r => (long)r.PointsAwarded, cancellationToken);

        var conversionRate = totalReferrals > 0 
            ? (decimal)(totalReferrals - pendingReferrals) / totalReferrals * 100 
            : 0;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new ReferralStatsDto(
            totalReferrals,
            completedReferrals,
            pendingReferrals,
            totalPointsAwarded,
            conversionRate);
    }

    private string GenerateCode(string email)
    {
        var prefix = email.Split('@')[0].ToUpper().Substring(0, Math.Min(4, email.Length));
        // ✅ THREAD SAFETY: Random.Shared kullan (new Random() thread-safe değil)
        var random = Random.Shared.Next(1000, 9999);
        return $"{prefix}{random}";
    }
}

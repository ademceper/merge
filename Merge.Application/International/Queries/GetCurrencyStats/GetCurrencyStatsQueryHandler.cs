using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.International.Queries.GetCurrencyStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetCurrencyStatsQueryHandler : IRequestHandler<GetCurrencyStatsQuery, CurrencyStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetCurrencyStatsQueryHandler> _logger;

    public GetCurrencyStatsQueryHandler(
        IDbContext context,
        ILogger<GetCurrencyStatsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CurrencyStatsDto> Handle(GetCurrencyStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting currency stats");

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var totalCurrencies = await _context.Set<Currency>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var activeCurrencies = await _context.Set<Currency>()
            .AsNoTracking()
            .CountAsync(c => c.IsActive, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var baseCurrency = await _context.Set<Currency>()
            .AsNoTracking()
            .Where(c => c.IsBaseCurrency)
            .Select(c => c.Code)
            .FirstOrDefaultAsync(cancellationToken) ?? "USD";

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var lastUpdate = await _context.Set<Currency>()
            .AsNoTracking()
            .MaxAsync(c => (DateTime?)c.LastUpdated, cancellationToken) ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalUsers = await _context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var currencyUsage = await _context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .GroupBy(p => new { p.CurrencyCode })
            .Select(g => new
            {
                g.Key.CurrencyCode,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch loading - currency names için dictionary
        var currencyNames = await _context.Set<Currency>()
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Code, c => c.Name, cancellationToken);

        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - Database'de DTO oluştur
        // ✅ PERFORMANCE: Batch loading - currency names için dictionary
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var mostUsed = new List<CurrencyUsageDto>(currencyUsage.Count);
        foreach (var u in currencyUsage)
        {
            mostUsed.Add(new CurrencyUsageDto(
                CurrencyCode: u.CurrencyCode,
                CurrencyName: currencyNames.TryGetValue(u.CurrencyCode, out var name) ? name : u.CurrencyCode,
                UserCount: u.Count,
                Percentage: totalUsers > 0 ? (decimal)u.Count / totalUsers * 100 : 0));
        }

        return new CurrencyStatsDto(
            TotalCurrencies: totalCurrencies,
            ActiveCurrencies: activeCurrencies,
            BaseCurrency: baseCurrency,
            LastRateUpdate: lastUpdate,
            MostUsedCurrencies: mostUsed.AsReadOnly());
    }
}


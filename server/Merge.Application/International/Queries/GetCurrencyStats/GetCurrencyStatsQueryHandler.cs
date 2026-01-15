using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetCurrencyStats;

public class GetCurrencyStatsQueryHandler(
    IDbContext context,
    ILogger<GetCurrencyStatsQueryHandler> logger) : IRequestHandler<GetCurrencyStatsQuery, CurrencyStatsDto>
{
    public async Task<CurrencyStatsDto> Handle(GetCurrencyStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting currency stats");

        var totalCurrencies = await context.Set<Currency>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeCurrencies = await context.Set<Currency>()
            .AsNoTracking()
            .CountAsync(c => c.IsActive, cancellationToken);

        var baseCurrency = await context.Set<Currency>()
            .AsNoTracking()
            .Where(c => c.IsBaseCurrency)
            .Select(c => c.Code)
            .FirstOrDefaultAsync(cancellationToken) ?? "USD";

        var lastUpdate = await context.Set<Currency>()
            .AsNoTracking()
            .MaxAsync(c => (DateTime?)c.LastUpdated, cancellationToken) ?? DateTime.UtcNow;

        var totalUsers = await context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var currencyUsage = await context.Set<UserCurrencyPreference>()
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

        var currencyNames = await context.Set<Currency>()
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Code, c => c.Name, cancellationToken);

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

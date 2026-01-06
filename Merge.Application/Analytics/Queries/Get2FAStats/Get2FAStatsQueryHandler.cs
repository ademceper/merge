using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Analytics.Queries.Get2FAStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class Get2FAStatsQueryHandler : IRequestHandler<Get2FAStatsQuery, TwoFactorStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<Get2FAStatsQueryHandler> _logger;

    public Get2FAStatsQueryHandler(
        IDbContext context,
        ILogger<Get2FAStatsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TwoFactorStatsDto> Handle(Get2FAStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching 2FA stats");

        // ✅ PERFORMANCE: Database'de toplam hesapla (memory'de Sum YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted and !t.IsDeleted checks (Global Query Filter handles it)
        var totalUsers = await _context.Users.AsNoTracking().CountAsync(cancellationToken);
        
        var twoFactorQuery = _context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .Where(t => t.IsEnabled);

        var usersWithTwoFactorCount = await twoFactorQuery.CountAsync(cancellationToken);
        
        var usersWithTwoFactor = await twoFactorQuery
            .GroupBy(t => t.Method)
            // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
            .Select(g => new TwoFactorMethodCount(
                g.Key.ToString(),
                g.Count(),
                totalUsers > 0 ? (g.Count() * 100.0m / totalUsers) : 0
            ))
            .ToListAsync(cancellationToken);

        var stats = new TwoFactorStatsDto(
            TotalUsers: totalUsers,
            UsersWithTwoFactor: usersWithTwoFactorCount,
            TwoFactorPercentage: totalUsers > 0 ? (usersWithTwoFactorCount * 100.0m / totalUsers) : 0,
            MethodBreakdown: usersWithTwoFactor
        );

        return stats;
    }
}


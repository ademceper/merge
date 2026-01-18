using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.Get2FAStats;

public class Get2FAStatsQueryHandler(
    IDbContext context,
    ILogger<Get2FAStatsQueryHandler> logger) : IRequestHandler<Get2FAStatsQuery, TwoFactorStatsDto>
{

    public async Task<TwoFactorStatsDto> Handle(Get2FAStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching 2FA stats");

        var totalUsers = await context.Users.AsNoTracking().CountAsync(cancellationToken);
        
        var twoFactorQuery = context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .Where(t => t.IsEnabled);

        var usersWithTwoFactorCount = await twoFactorQuery.CountAsync(cancellationToken);
        
        var usersWithTwoFactor = await twoFactorQuery
            .GroupBy(t => t.Method)
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


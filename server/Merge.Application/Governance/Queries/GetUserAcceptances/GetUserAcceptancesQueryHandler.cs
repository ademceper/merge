using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Governance;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Governance.Queries.GetUserAcceptances;

public class GetUserAcceptancesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetUserAcceptancesQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetUserAcceptancesQuery, IEnumerable<PolicyAcceptanceDto>>
{
    private const string CACHE_KEY_USER_ACCEPTANCES = "user_acceptances_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<IEnumerable<PolicyAcceptanceDto>> Handle(GetUserAcceptancesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving user acceptances. UserId: {UserId}", request.UserId);

        var cacheKey = $"{CACHE_KEY_USER_ACCEPTANCES}{request.UserId}";

        var cachedAcceptances = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for user acceptances. UserId: {UserId}", request.UserId);

                var acceptances = await context.Set<PolicyAcceptance>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(pa => pa.Policy)
                    .Include(pa => pa.User)
                    .Where(pa => pa.UserId == request.UserId)
                    .OrderByDescending(pa => pa.AcceptedAt)
                    .Take(500)
                    .ToListAsync(cancellationToken);

                List<PolicyAcceptanceDto> result = [];
                foreach (var acceptance in acceptances)
                {
                    result.Add(mapper.Map<PolicyAcceptanceDto>(acceptance));
                }
                return result;
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedAcceptances ?? [];
    }
}


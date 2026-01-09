using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Governance;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Queries.GetUserAcceptances;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUserAcceptancesQueryHandler : IRequestHandler<GetUserAcceptancesQuery, IEnumerable<PolicyAcceptanceDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserAcceptancesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_USER_ACCEPTANCES = "user_acceptances_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetUserAcceptancesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetUserAcceptancesQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<PolicyAcceptanceDto>> Handle(GetUserAcceptancesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving user acceptances. UserId: {UserId}", request.UserId);

        var cacheKey = $"{CACHE_KEY_USER_ACCEPTANCES}{request.UserId}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedAcceptances = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for user acceptances. UserId: {UserId}", request.UserId);

                // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
                var acceptances = await _context.Set<PolicyAcceptance>()
                    .AsNoTracking()
                    .Include(pa => pa.Policy)
                    .Include(pa => pa.User)
                    .Where(pa => pa.UserId == request.UserId)
                    .OrderByDescending(pa => pa.AcceptedAt)
                    .Take(500) // ✅ Güvenlik: Maksimum 500 acceptance
                    .ToListAsync(cancellationToken);

                // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
                // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
                var result = new List<PolicyAcceptanceDto>(acceptances.Count);
                foreach (var acceptance in acceptances)
                {
                    result.Add(_mapper.Map<PolicyAcceptanceDto>(acceptance));
                }
                return result;
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedAcceptances ?? Enumerable.Empty<PolicyAcceptanceDto>();
    }
}


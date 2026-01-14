using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using System.Text;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetRobotsTxt;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetRobotsTxtQueryHandler : IRequestHandler<GetRobotsTxtQuery, string>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetRobotsTxtQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ROBOTS_TXT = "robots_txt";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1); // Robots.txt changes less frequently

    public GetRobotsTxtQueryHandler(
        IDbContext context,
        ILogger<GetRobotsTxtQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<string> Handle(GetRobotsTxtQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating robots.txt");

        // ✅ BOLUM 10.2: Redis distributed cache for robots.txt
        var cachedContent = await _cache.GetOrCreateAsync(
            CACHE_KEY_ROBOTS_TXT,
            async () =>
            {
                _logger.LogInformation("Cache miss for robots.txt. Generating from database.");

                var sb = new StringBuilder();
                sb.AppendLine("User-agent: *");
                sb.AppendLine("Allow: /");
                
                // Add disallow rules for deleted/inactive pages
                // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
                var disallowedEntries = await _context.Set<SEOSettings>()
                    .AsNoTracking()
                    .Where(s => !s.IsIndexed)
                    .ToListAsync(cancellationToken);

                foreach (var entry in disallowedEntries)
                {
                    if (!string.IsNullOrEmpty(entry.CanonicalUrl))
                    {
                        sb.AppendLine($"Disallow: {entry.CanonicalUrl}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("Sitemap: /sitemap.xml");

                return sb.ToString();
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedContent!;
    }
}


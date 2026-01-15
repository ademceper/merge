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

public class GetRobotsTxtQueryHandler(
    IDbContext context,
    ILogger<GetRobotsTxtQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetRobotsTxtQuery, string>
{
    private const string CACHE_KEY_ROBOTS_TXT = "robots_txt";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1);

    public async Task<string> Handle(GetRobotsTxtQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating robots.txt");

        var cachedContent = await cache.GetOrCreateAsync(
            CACHE_KEY_ROBOTS_TXT,
            async () =>
            {
                logger.LogInformation("Cache miss for robots.txt. Generating from database.");

                var sb = new StringBuilder();
                sb.AppendLine("User-agent: *");
                sb.AppendLine("Allow: /");
                
                var disallowedEntries = await context.Set<SEOSettings>()
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


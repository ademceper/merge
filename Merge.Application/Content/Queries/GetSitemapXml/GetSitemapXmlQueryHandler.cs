using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using System.Xml.Linq;

namespace Merge.Application.Content.Queries.GetSitemapXml;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetSitemapXmlQueryHandler : IRequestHandler<GetSitemapXmlQuery, string>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetSitemapXmlQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_SITEMAP_XML = "sitemap_xml";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1); // Sitemap XML changes less frequently

    public GetSitemapXmlQueryHandler(
        IDbContext context,
        ILogger<GetSitemapXmlQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<string> Handle(GetSitemapXmlQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating sitemap XML");

        // ✅ BOLUM 10.2: Redis distributed cache for sitemap XML
        var cachedXml = await _cache.GetOrCreateAsync(
            CACHE_KEY_SITEMAP_XML,
            async () =>
            {
                _logger.LogInformation("Cache miss for sitemap XML. Generating from database.");

                // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
                var entries = await _context.Set<SitemapEntry>()
                    .AsNoTracking()
                    .Where(e => e.IsActive)
                    .ToListAsync(cancellationToken);

                var xNamespace = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
                var sitemap = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement(xNamespace + "urlset",
                        entries.Select(e => new XElement(xNamespace + "url",
                            new XElement(xNamespace + "loc", e.Url),
                            new XElement(xNamespace + "lastmod", e.LastModified.ToString("yyyy-MM-dd")),
                            new XElement(xNamespace + "changefreq", e.ChangeFrequency),
                            new XElement(xNamespace + "priority", e.Priority.ToString("F1"))
                        ))
                    )
                );

                return sitemap.ToString();
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedXml!;
    }
}


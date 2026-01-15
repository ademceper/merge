using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using System.Xml.Linq;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetSitemapXml;

public class GetSitemapXmlQueryHandler(
    IDbContext context,
    ILogger<GetSitemapXmlQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetSitemapXmlQuery, string>
{
    private const string CACHE_KEY_SITEMAP_XML = "sitemap_xml";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1);

    public async Task<string> Handle(GetSitemapXmlQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating sitemap XML");

        var cachedXml = await cache.GetOrCreateAsync(
            CACHE_KEY_SITEMAP_XML,
            async () =>
            {
                logger.LogInformation("Cache miss for sitemap XML. Generating from database.");

                var entries = await context.Set<SitemapEntry>()
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


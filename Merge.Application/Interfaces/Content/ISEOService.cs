using Merge.Application.DTOs.Content;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Content;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
public interface ISEOService
{
    [Obsolete("Use CreateOrUpdateSEOSettingsCommand via MediatR instead")]
    Task<SEOSettingsDto> CreateOrUpdateSEOSettingsAsync(object dto, CancellationToken cancellationToken = default);
    Task<SEOSettingsDto?> GetSEOSettingsAsync(string pageType, Guid? entityId = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteSEOSettingsAsync(string pageType, Guid entityId, CancellationToken cancellationToken = default);
    
    // Sitemap
    Task<SitemapEntryDto> AddSitemapEntryAsync(string url, string pageType, Guid? entityId = null, string changeFrequency = "weekly", decimal priority = 0.5m, CancellationToken cancellationToken = default);
    Task<bool> UpdateSitemapEntryAsync(Guid id, string? url = null, string? changeFrequency = null, decimal? priority = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveSitemapEntryAsync(Guid id, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<SitemapEntryDto>> GetAllSitemapEntriesAsync(bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<string> GenerateSitemapXmlAsync(CancellationToken cancellationToken = default); // Generate XML sitemap
    Task<string> GenerateRobotsTxtAsync(CancellationToken cancellationToken = default); // Generate robots.txt content
    
    // Auto-generate SEO for entities
    Task<SEOSettingsDto> GenerateSEOForProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<SEOSettingsDto> GenerateSEOForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<SEOSettingsDto> GenerateSEOForBlogPostAsync(Guid postId, CancellationToken cancellationToken = default);
}


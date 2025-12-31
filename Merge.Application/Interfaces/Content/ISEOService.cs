using Merge.Application.DTOs.Content;
namespace Merge.Application.Interfaces.Content;

public interface ISEOService
{
    Task<SEOSettingsDto> CreateOrUpdateSEOSettingsAsync(CreateSEOSettingsDto dto);
    Task<SEOSettingsDto?> GetSEOSettingsAsync(string pageType, Guid? entityId = null);
    Task<bool> DeleteSEOSettingsAsync(string pageType, Guid entityId);
    
    // Sitemap
    Task<SitemapEntryDto> AddSitemapEntryAsync(string url, string pageType, Guid? entityId = null, string changeFrequency = "weekly", decimal priority = 0.5m);
    Task<bool> UpdateSitemapEntryAsync(Guid id, string? url = null, string? changeFrequency = null, decimal? priority = null);
    Task<bool> RemoveSitemapEntryAsync(Guid id);
    Task<IEnumerable<SitemapEntryDto>> GetAllSitemapEntriesAsync(bool? isActive = null);
    Task<string> GenerateSitemapXmlAsync(); // Generate XML sitemap
    Task<string> GenerateRobotsTxtAsync(); // Generate robots.txt content
    
    // Auto-generate SEO for entities
    Task<SEOSettingsDto> GenerateSEOForProductAsync(Guid productId);
    Task<SEOSettingsDto> GenerateSEOForCategoryAsync(Guid categoryId);
    Task<SEOSettingsDto> GenerateSEOForBlogPostAsync(Guid postId);
}


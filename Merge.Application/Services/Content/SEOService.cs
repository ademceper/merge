using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Content;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Services.Content;

public class SEOService : ISEOService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SEOService> _logger;

    public SEOService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<SEOService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SEOSettingsDto> CreateOrUpdateSEOSettingsAsync(CreateSEOSettingsDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var existing = await _context.Set<SEOSettings>()
            .FirstOrDefaultAsync(s => s.PageType == dto.PageType && 
                                    s.EntityId == dto.EntityId, cancellationToken);

        if (existing != null)
        {
            existing.MetaTitle = dto.MetaTitle;
            existing.MetaDescription = dto.MetaDescription;
            existing.MetaKeywords = dto.MetaKeywords;
            existing.CanonicalUrl = dto.CanonicalUrl;
            existing.OgTitle = dto.OgTitle;
            existing.OgDescription = dto.OgDescription;
            existing.OgImageUrl = dto.OgImageUrl;
            existing.TwitterCard = dto.TwitterCard;
            existing.StructuredData = dto.StructuredData != null ? JsonSerializer.Serialize(dto.StructuredData) : null;
            existing.IsIndexed = dto.IsIndexed;
            existing.FollowLinks = dto.FollowLinks;
            existing.Priority = dto.Priority;
            existing.ChangeFrequency = dto.ChangeFrequency;
            existing.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<SEOSettingsDto>(existing);
        }

        var seoSettings = new SEOSettings
        {
            PageType = dto.PageType,
            EntityId = dto.EntityId,
            MetaTitle = dto.MetaTitle,
            MetaDescription = dto.MetaDescription,
            MetaKeywords = dto.MetaKeywords,
            CanonicalUrl = dto.CanonicalUrl,
            OgTitle = dto.OgTitle,
            OgDescription = dto.OgDescription,
            OgImageUrl = dto.OgImageUrl,
            TwitterCard = dto.TwitterCard,
            StructuredData = dto.StructuredData != null ? JsonSerializer.Serialize(dto.StructuredData) : null,
            IsIndexed = dto.IsIndexed,
            FollowLinks = dto.FollowLinks,
            Priority = dto.Priority,
            ChangeFrequency = dto.ChangeFrequency
        };

        await _context.Set<SEOSettings>().AddAsync(seoSettings, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SEOSettingsDto>(seoSettings);
    }

    public async Task<SEOSettingsDto?> GetSEOSettingsAsync(string pageType, Guid? entityId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var settings = await _context.Set<SEOSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.PageType == pageType && 
                                    s.EntityId == entityId, cancellationToken);

        return settings != null ? _mapper.Map<SEOSettingsDto>(settings) : null;
    }

    public async Task<bool> DeleteSEOSettingsAsync(string pageType, Guid entityId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var settings = await _context.Set<SEOSettings>()
            .FirstOrDefaultAsync(s => s.PageType == pageType && 
                                    s.EntityId == entityId, cancellationToken);

        if (settings == null) return false;

        settings.IsDeleted = true;
        settings.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Sitemap
    public async Task<SitemapEntryDto> AddSitemapEntryAsync(string url, string pageType, Guid? entityId = null, string changeFrequency = "weekly", decimal priority = 0.5m, CancellationToken cancellationToken = default)
    {
        var entry = new SitemapEntry
        {
            Url = url,
            PageType = pageType,
            EntityId = entityId,
            ChangeFrequency = changeFrequency,
            Priority = priority,
            LastModified = DateTime.UtcNow,
            IsActive = true
        };

        await _context.Set<SitemapEntry>().AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SitemapEntryDto>(entry);
    }

    public async Task<bool> UpdateSitemapEntryAsync(Guid id, string? url = null, string? changeFrequency = null, decimal? priority = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted (Global Query Filter)
        var entry = await _context.Set<SitemapEntry>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entry == null) return false;

        if (!string.IsNullOrEmpty(url))
            entry.Url = url;
        if (!string.IsNullOrEmpty(changeFrequency))
            entry.ChangeFrequency = changeFrequency;
        if (priority.HasValue)
            entry.Priority = priority.Value;

        entry.LastModified = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RemoveSitemapEntryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted (Global Query Filter)
        var entry = await _context.Set<SitemapEntry>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entry == null) return false;

        entry.IsDeleted = true;
        entry.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    public async Task<PagedResult<SitemapEntryDto>> GetAllSitemapEntriesAsync(bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        var query = _context.Set<SitemapEntry>()
            .AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(e => e.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var entries = await query
            .OrderBy(e => e.PageType)
            .ThenBy(e => e.Priority)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = entries.Select(e => _mapper.Map<SitemapEntryDto>(e)).ToList();

        return new PagedResult<SitemapEntryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<string> GenerateSitemapXmlAsync(CancellationToken cancellationToken = default)
    {
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
    }

    public async Task<string> GenerateRobotsTxtAsync(CancellationToken cancellationToken = default)
    {
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
    }

    // Auto-generate SEO
    public async Task<SEOSettingsDto> GenerateSEOForProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", productId);
        }

        var metaTitle = $"{product.Name} - {product.Category?.Name ?? "Product"}";
        var metaDescription = !string.IsNullOrEmpty(product.Description) 
            ? product.Description.Length > 160 
                ? product.Description.Substring(0, 157) + "..." 
                : product.Description
            : $"Buy {product.Name} online. Best price and quality guaranteed.";

        var dto = new CreateSEOSettingsDto
        {
            PageType = "Product",
            EntityId = productId,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = $"{product.Name}, {product.Category?.Name}, {product.Brand}",
            CanonicalUrl = $"/products/{product.SKU}",
            OgTitle = metaTitle,
            OgDescription = metaDescription,
            OgImageUrl = product.ImageUrl,
            IsIndexed = true,
            FollowLinks = true,
            Priority = 0.8m,
            ChangeFrequency = "weekly"
        };

        return await CreateOrUpdateSEOSettingsAsync(dto, cancellationToken);
    }

    public async Task<SEOSettingsDto> GenerateSEOForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException("Kategori", categoryId);
        }

        var metaTitle = $"{category.Name} - Shop Online";
        var metaDescription = !string.IsNullOrEmpty(category.Description)
            ? category.Description.Length > 160
                ? category.Description.Substring(0, 157) + "..."
                : category.Description
            : $"Browse {category.Name} products. Wide selection and best prices.";

        var dto = new CreateSEOSettingsDto
        {
            PageType = "Category",
            EntityId = categoryId,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = category.Name,
            CanonicalUrl = $"/categories/{category.Slug}",
            IsIndexed = true,
            FollowLinks = true,
            Priority = 0.7m,
            ChangeFrequency = "daily"
        };

        return await CreateOrUpdateSEOSettingsAsync(dto, cancellationToken);
    }

    public async Task<SEOSettingsDto> GenerateSEOForBlogPostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException("Blog yazısı", postId);
        }

        var metaTitle = post.MetaTitle ?? post.Title;
        var metaDescription = post.MetaDescription ?? post.Excerpt;

        var dto = new CreateSEOSettingsDto
        {
            PageType = "Blog",
            EntityId = postId,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = post.MetaKeywords ?? post.Tags,
            CanonicalUrl = $"/blog/{post.Slug}",
            OgTitle = metaTitle,
            OgDescription = metaDescription,
            OgImageUrl = post.OgImageUrl ?? post.FeaturedImageUrl,
            IsIndexed = post.Status == ContentStatus.Published,
            FollowLinks = true,
            Priority = 0.6m,
            ChangeFrequency = "weekly"
        };

        return await CreateOrUpdateSEOSettingsAsync(dto, cancellationToken);
    }

}


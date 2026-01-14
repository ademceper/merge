using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Enums;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Content;

public class SEOService : ISEOService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SEOService> _logger;

    public SEOService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<SEOService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    [Obsolete("Use CreateOrUpdateSEOSettingsCommand via MediatR instead")]
    public async Task<SEOSettingsDto> CreateOrUpdateSEOSettingsAsync(object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateSEOSettingsDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        _logger.LogInformation("SEO ayarlari olusturuluyor/guncelleniyor. PageType: {PageType}, EntityId: {EntityId}", 
            dto.PageType, dto.EntityId);

        try
        {
            // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
            var existing = await _context.Set<SEOSettings>()
                .FirstOrDefaultAsync(s => s.PageType == dto.PageType && 
                                        s.EntityId == dto.EntityId, cancellationToken);

            if (existing != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                existing.UpdateMetaInformation(
                    dto.MetaTitle,
                    dto.MetaDescription,
                    dto.MetaKeywords,
                    dto.CanonicalUrl);

                existing.UpdateOpenGraphInformation(
                    dto.OgTitle,
                    dto.OgDescription,
                    dto.OgImageUrl,
                    dto.TwitterCard);

                existing.UpdateStructuredData(dto.StructuredDataJson);
                existing.UpdateIndexingSettings(dto.IsIndexed, dto.FollowLinks);
                existing.UpdateSitemapSettings(dto.Priority, dto.ChangeFrequency);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("SEO ayarlari guncellendi. SEOSettingsId: {SEOSettingsId}, PageType: {PageType}", 
                    existing.Id, dto.PageType);

                return _mapper.Map<SEOSettingsDto>(existing);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var seoSettings = SEOSettings.Create(
                dto.PageType,
                dto.EntityId,
                dto.MetaTitle,
                dto.MetaDescription,
                dto.MetaKeywords,
                dto.CanonicalUrl,
                dto.OgTitle,
                dto.OgDescription,
                dto.OgImageUrl,
                dto.TwitterCard,
                dto.StructuredDataJson,
                dto.IsIndexed,
                dto.FollowLinks,
                dto.Priority,
                dto.ChangeFrequency);

            await _context.Set<SEOSettings>().AddAsync(seoSettings, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("SEO ayarlari olusturuldu. SEOSettingsId: {SEOSettingsId}, PageType: {PageType}", 
                seoSettings.Id, dto.PageType);

            return _mapper.Map<SEOSettingsDto>(seoSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SEO ayarlari olusturma/guncelleme hatasi. PageType: {PageType}, EntityId: {EntityId}", 
                dto.PageType, dto.EntityId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
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

        settings.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Sitemap
    public async Task<SitemapEntryDto> AddSitemapEntryAsync(string url, string pageType, Guid? entityId = null, string changeFrequency = "weekly", decimal priority = 0.5m, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var entry = SitemapEntry.Create(
            url,
            pageType,
            entityId,
            changeFrequency,
            priority,
            isActive: true);

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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (!string.IsNullOrEmpty(url))
            entry.UpdateUrl(url);
        if (!string.IsNullOrEmpty(changeFrequency) || priority.HasValue)
            entry.UpdateSitemapSettings(changeFrequency ?? entry.ChangeFrequency, priority ?? entry.Priority);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RemoveSitemapEntryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted (Global Query Filter)
        var entry = await _context.Set<SitemapEntry>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entry == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        entry.MarkAsDeleted();
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
        var product = await _context.Set<ProductEntity>()
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

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public async Task<SEOSettingsDto> GenerateSEOForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<Category>()
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

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete
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

#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete
    }

}


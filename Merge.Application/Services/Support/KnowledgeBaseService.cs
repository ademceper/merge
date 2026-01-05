using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Support;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Services.Support;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<KnowledgeBaseService> _logger;

    public KnowledgeBaseService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<KnowledgeBaseService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<KnowledgeBaseArticleDto> CreateArticleAsync(CreateKnowledgeBaseArticleDto dto, Guid authorId, CancellationToken cancellationToken = default)
    {
        var slug = GenerateSlug(dto.Title);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        // Ensure unique slug
        var existingSlug = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .AnyAsync(a => a.Slug == slug, cancellationToken);
        
        if (existingSlug)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var article = new KnowledgeBaseArticle
        {
            Title = dto.Title,
            Slug = slug,
            Content = dto.Content,
            Excerpt = dto.Excerpt,
            CategoryId = dto.CategoryId,
            Status = Enum.Parse<ContentStatus>(dto.Status),
            IsFeatured = dto.IsFeatured,
            DisplayOrder = dto.DisplayOrder,
            Tags = dto.Tags != null ? string.Join(",", dto.Tags) : null,
            AuthorId = authorId,
            PublishedAt = dto.Status == nameof(ContentStatus.Published) ? DateTime.UtcNow : null
        };

        await _context.Set<KnowledgeBaseArticle>().AddAsync(article, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes for mapping
        article = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == article.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<KnowledgeBaseArticleDto>(article!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<KnowledgeBaseArticleDto?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var article = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return article != null ? _mapper.Map<KnowledgeBaseArticleDto>(article) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<KnowledgeBaseArticleDto?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var article = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Slug == slug && a.Status == ContentStatus.Published, cancellationToken);

        return article != null ? _mapper.Map<KnowledgeBaseArticleDto>(article) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<KnowledgeBaseArticleDto>> GetArticlesAsync(string? status = null, Guid? categoryId = null, bool featuredOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        IQueryable<KnowledgeBaseArticle> query = _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<ContentStatus>(status);
            query = query.Where(a => a.Status == statusEnum);
        }
        else
        {
            query = query.Where(a => a.Status == ContentStatus.Published);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == categoryId.Value);
        }

        if (featuredOnly)
        {
            query = query.Where(a => a.IsFeatured);
        }

        var articles = await query
            .OrderBy(a => a.DisplayOrder)
            .ThenByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<KnowledgeBaseArticleDto>>(articles);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<KnowledgeBaseArticleDto>> SearchArticlesAsync(KnowledgeBaseSearchDto searchDto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        IQueryable<KnowledgeBaseArticle> query = _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.Status == ContentStatus.Published);

        if (!string.IsNullOrEmpty(searchDto.Query))
        {
            query = query.Where(a => 
                a.Title.Contains(searchDto.Query) ||
                a.Content.Contains(searchDto.Query) ||
                a.Excerpt != null && a.Excerpt.Contains(searchDto.Query) ||
                a.Tags != null && a.Tags.Contains(searchDto.Query));
        }

        if (searchDto.CategoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == searchDto.CategoryId.Value);
        }

        if (searchDto.FeaturedOnly)
        {
            query = query.Where(a => a.IsFeatured);
        }

        var articles = await query
            .OrderByDescending(a => a.IsFeatured)
            .ThenByDescending(a => a.ViewCount)
            .ThenByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<KnowledgeBaseArticleDto>>(articles);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<KnowledgeBaseArticleDto> UpdateArticleAsync(Guid id, UpdateKnowledgeBaseArticleDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (article == null)
        {
            throw new NotFoundException("Makale", id);
        }

        if (!string.IsNullOrEmpty(dto.Title))
        {
            article.Title = dto.Title;
            article.Slug = GenerateSlug(dto.Title);
        }
        if (!string.IsNullOrEmpty(dto.Content))
            article.Content = dto.Content;
        if (dto.Excerpt != null)
            article.Excerpt = dto.Excerpt;
        if (dto.CategoryId.HasValue)
            article.CategoryId = dto.CategoryId.Value;
        if (!string.IsNullOrEmpty(dto.Status))
        {
            article.Status = Enum.Parse<ContentStatus>(dto.Status);
            if (dto.Status == nameof(ContentStatus.Published) && !article.PublishedAt.HasValue)
            {
                article.PublishedAt = DateTime.UtcNow;
            }
        }
        if (dto.IsFeatured.HasValue)
            article.IsFeatured = dto.IsFeatured.Value;
        if (dto.DisplayOrder.HasValue)
            article.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.Tags != null)
            article.Tags = string.Join(",", dto.Tags);

        article.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes for mapping
        article = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == article.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<KnowledgeBaseArticleDto>(article!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (article == null) return false;

        article.IsDeleted = true;
        article.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> PublishArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (article == null) return false;

        article.Status = ContentStatus.Published;
        article.PublishedAt = DateTime.UtcNow;
        article.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task RecordArticleViewAsync(Guid articleId, Guid? userId = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId, cancellationToken);

        if (article == null) return;

        var view = new KnowledgeBaseView
        {
            ArticleId = articleId,
            UserId = userId,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = string.Empty
        };

        await _context.Set<KnowledgeBaseView>().AddAsync(view, cancellationToken);
        article.ViewCount++;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<KnowledgeBaseCategoryDto> CreateCategoryAsync(CreateKnowledgeBaseCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var slug = GenerateSlug(dto.Name);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        // Ensure unique slug
        var existingSlug = await _context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .AnyAsync(c => c.Slug == slug, cancellationToken);
        
        if (existingSlug)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var category = new KnowledgeBaseCategory
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            IconUrl = dto.IconUrl
        };

        await _context.Set<KnowledgeBaseCategory>().AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes for mapping
        category = await _context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return await MapToCategoryDtoAsync(category!, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<KnowledgeBaseCategoryDto?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var category = await _context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return category != null ? await MapToCategoryDtoAsync(category, cancellationToken) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<KnowledgeBaseCategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var category = await _context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive, cancellationToken);

        return category != null ? await MapToCategoryDtoAsync(category, cancellationToken) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<KnowledgeBaseCategoryDto>> GetCategoriesAsync(bool includeSubCategories = true, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        IQueryable<KnowledgeBaseCategory> query = _context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .Where(c => c.IsActive);

        if (includeSubCategories)
        {
            query = query.Include(c => c.SubCategories.Where(sc => sc.IsActive));
        }

        // ✅ PERFORMANCE: categoryIds'i database'de oluştur, memory'de işlem YASAK
        var categoryIds = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load article counts for all categories to avoid N+1 query
        var articleCountsDict = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Where(a => a.CategoryId.HasValue && categoryIds.Contains(a.CategoryId.Value) && a.Status == ContentStatus.Published)
            .GroupBy(a => a.CategoryId!.Value)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        var result = new List<KnowledgeBaseCategoryDto>(categories.Count);
        foreach (var category in categories)
        {
            var dto = _mapper.Map<KnowledgeBaseCategoryDto>(category);
            
            // ✅ PERFORMANCE: Set ArticleCount from batch loaded dictionary
            if (articleCountsDict.TryGetValue(category.Id, out var count))
            {
                dto.ArticleCount = count;
            }
            else
            {
                dto.ArticleCount = 0;
            }
            
            // ✅ PERFORMANCE: Recursively map subcategories if needed
            if (includeSubCategories && category.SubCategories != null && category.SubCategories.Any())
            {
                dto.SubCategories = await MapSubCategoriesAsync(category.SubCategories.ToList(), articleCountsDict, cancellationToken);
            }
            else
            {
                dto.SubCategories = new List<KnowledgeBaseCategoryDto>();
            }
            
            result.Add(dto);
        }
        return result;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<KnowledgeBaseCategoryDto> UpdateCategoryAsync(Guid id, UpdateKnowledgeBaseCategoryDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var category = await _context.Set<KnowledgeBaseCategory>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException("Kategori", id);
        }

        if (!string.IsNullOrEmpty(dto.Name))
        {
            category.Name = dto.Name;
            category.Slug = GenerateSlug(dto.Name);
        }
        if (dto.Description != null)
            category.Description = dto.Description;
        if (dto.ParentCategoryId.HasValue)
            category.ParentCategoryId = dto.ParentCategoryId.Value;
        if (dto.DisplayOrder.HasValue)
            category.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;
        if (dto.IconUrl != null)
            category.IconUrl = dto.IconUrl;

        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes for mapping
        category = await _context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return await MapToCategoryDtoAsync(category!, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var category = await _context.Set<KnowledgeBaseCategory>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null) return false;

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<int> GetArticleCountAsync(Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        IQueryable<KnowledgeBaseArticle> query = _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Where(a => a.Status == ContentStatus.Published);

        if (categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == categoryId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<int> GetTotalViewsAsync(Guid? articleId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        IQueryable<KnowledgeBaseView> query = _context.Set<KnowledgeBaseView>()
            .AsNoTracking();

        if (articleId.HasValue)
        {
            query = query.Where(v => v.ArticleId == articleId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    private string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "");

        // Remove multiple dashes
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<KnowledgeBaseCategoryDto> MapToCategoryDtoAsync(KnowledgeBaseCategory category, CancellationToken cancellationToken = default)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan
        var dto = _mapper.Map<KnowledgeBaseCategoryDto>(category);

        // ✅ PERFORMANCE: Batch load article count to avoid N+1 query
        dto.ArticleCount = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .CountAsync(a => a.CategoryId == category.Id && a.Status == ContentStatus.Published, cancellationToken);

        // ✅ PERFORMANCE: Recursively map subcategories if needed
        if (category.SubCategories != null && category.SubCategories.Any())
        {
            // ✅ PERFORMANCE: Navigation property'den ID'leri al (zaten Include ile yüklenmiş, memory'de Select kabul edilebilir)
            var subCategoryIds = category.SubCategories.Select(sc => sc.Id).ToList();
            var subArticleCountsDict = await _context.Set<KnowledgeBaseArticle>()
                .AsNoTracking()
                .Where(a => a.CategoryId.HasValue && subCategoryIds.Contains(a.CategoryId.Value) && a.Status == ContentStatus.Published)
                .GroupBy(a => a.CategoryId!.Value)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

            dto.SubCategories = await MapSubCategoriesAsync(category.SubCategories.ToList(), subArticleCountsDict, cancellationToken);
        }
        else
        {
            dto.SubCategories = new List<KnowledgeBaseCategoryDto>();
        }

        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<List<KnowledgeBaseCategoryDto>> MapSubCategoriesAsync(List<KnowledgeBaseCategory> subCategories, Dictionary<Guid, int> articleCountsDict, CancellationToken cancellationToken = default)
    {
        var result = new List<KnowledgeBaseCategoryDto>(subCategories.Count);
        foreach (var sc in subCategories.Where(sc => sc.IsActive))
        {
            var subDto = _mapper.Map<KnowledgeBaseCategoryDto>(sc);
            
            // ✅ PERFORMANCE: Set ArticleCount from batch loaded dictionary
            if (articleCountsDict.TryGetValue(sc.Id, out var count))
            {
                subDto.ArticleCount = count;
            }
            else
            {
                subDto.ArticleCount = 0;
            }
            
            // ✅ PERFORMANCE: Recursively map nested subcategories if needed
            if (sc.SubCategories != null && sc.SubCategories.Any())
            {
                // ✅ NestedSubCategoryIds'i memory'den al (zaten yüklenmiş navigation property)
                var nestedSubCategoryIds = sc.SubCategories.Select(nsc => nsc.Id).ToList();
                var nestedSubArticleCountsDict = await _context.Set<KnowledgeBaseArticle>()
                    .AsNoTracking()
                    .Where(a => a.CategoryId.HasValue && nestedSubCategoryIds.Contains(a.CategoryId.Value) && a.Status == ContentStatus.Published)
                    .GroupBy(a => a.CategoryId!.Value)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

                subDto.SubCategories = await MapSubCategoriesAsync(sc.SubCategories.ToList(), nestedSubArticleCountsDict, cancellationToken);
            }
            else
            {
                subDto.SubCategories = new List<KnowledgeBaseCategoryDto>();
            }
            
            result.Add(subDto);
        }
        return result;
    }
}


using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Support;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Services.Support;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public KnowledgeBaseService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<KnowledgeBaseArticleDto> CreateArticleAsync(CreateKnowledgeBaseArticleDto dto, Guid authorId)
    {
        var slug = GenerateSlug(dto.Title);

        // Ensure unique slug
        var existingSlug = await _context.Set<KnowledgeBaseArticle>()
            .AnyAsync(a => a.Slug == slug && !a.IsDeleted);
        
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
            Status = dto.Status,
            IsFeatured = dto.IsFeatured,
            DisplayOrder = dto.DisplayOrder,
            Tags = dto.Tags != null ? string.Join(",", dto.Tags) : null,
            AuthorId = authorId,
            PublishedAt = dto.Status == "Published" ? DateTime.UtcNow : null
        };

        await _context.Set<KnowledgeBaseArticle>().AddAsync(article);
        await _unitOfWork.SaveChangesAsync();

        return await MapToArticleDto(article);
    }

    public async Task<KnowledgeBaseArticleDto?> GetArticleAsync(Guid id)
    {
        var article = await _context.Set<KnowledgeBaseArticle>()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        return article != null ? await MapToArticleDto(article) : null;
    }

    public async Task<KnowledgeBaseArticleDto?> GetArticleBySlugAsync(string slug)
    {
        var article = await _context.Set<KnowledgeBaseArticle>()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Slug == slug && !a.IsDeleted && a.Status == "Published");

        return article != null ? await MapToArticleDto(article) : null;
    }

    public async Task<IEnumerable<KnowledgeBaseArticleDto>> GetArticlesAsync(string? status = null, Guid? categoryId = null, bool featuredOnly = false, int page = 1, int pageSize = 20)
    {
        var query = _context.Set<KnowledgeBaseArticle>()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }
        else
        {
            query = query.Where(a => a.Status == "Published");
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
            .ToListAsync();

        var result = new List<KnowledgeBaseArticleDto>();
        foreach (var article in articles)
        {
            result.Add(await MapToArticleDto(article));
        }
        return result;
    }

    public async Task<IEnumerable<KnowledgeBaseArticleDto>> SearchArticlesAsync(KnowledgeBaseSearchDto searchDto)
    {
        var query = _context.Set<KnowledgeBaseArticle>()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => !a.IsDeleted && a.Status == "Published");

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
            .AsNoTracking() // Performance: Read-only query, no change tracking needed
            .OrderByDescending(a => a.IsFeatured)
            .ThenByDescending(a => a.ViewCount)
            .ThenByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        var result = new List<KnowledgeBaseArticleDto>();
        foreach (var article in articles)
        {
            result.Add(await MapToArticleDto(article));
        }
        return result;
    }

    public async Task<KnowledgeBaseArticleDto> UpdateArticleAsync(Guid id, UpdateKnowledgeBaseArticleDto dto)
    {
        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

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
            article.Status = dto.Status;
            if (dto.Status == "Published" && !article.PublishedAt.HasValue)
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
        await _unitOfWork.SaveChangesAsync();

        return await MapToArticleDto(article);
    }

    public async Task<bool> DeleteArticleAsync(Guid id)
    {
        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (article == null) return false;

        article.IsDeleted = true;
        article.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PublishArticleAsync(Guid id)
    {
        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (article == null) return false;

        article.Status = "Published";
        article.PublishedAt = DateTime.UtcNow;
        article.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task RecordArticleViewAsync(Guid articleId, Guid? userId = null, string? ipAddress = null)
    {
        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId && !a.IsDeleted);

        if (article == null) return;

        var view = new KnowledgeBaseView
        {
            ArticleId = articleId,
            UserId = userId,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = string.Empty
        };

        await _context.Set<KnowledgeBaseView>().AddAsync(view);
        article.ViewCount++;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<KnowledgeBaseCategoryDto> CreateCategoryAsync(CreateKnowledgeBaseCategoryDto dto)
    {
        var slug = GenerateSlug(dto.Name);

        // Ensure unique slug
        var existingSlug = await _context.Set<KnowledgeBaseCategory>()
            .AnyAsync(c => c.Slug == slug && !c.IsDeleted);
        
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

        await _context.Set<KnowledgeBaseCategory>().AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return await MapToCategoryDto(category);
    }

    public async Task<KnowledgeBaseCategoryDto?> GetCategoryAsync(Guid id)
    {
        var category = await _context.Set<KnowledgeBaseCategory>()
            .Include(c => c.ParentCategory)
            .Include(c => c.Articles.Where(a => !a.IsDeleted && a.Status == "Published"))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return category != null ? await MapToCategoryDto(category) : null;
    }

    public async Task<KnowledgeBaseCategoryDto?> GetCategoryBySlugAsync(string slug)
    {
        var category = await _context.Set<KnowledgeBaseCategory>()
            .Include(c => c.ParentCategory)
            .Include(c => c.Articles.Where(a => !a.IsDeleted && a.Status == "Published"))
            .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted && c.IsActive);

        return category != null ? await MapToCategoryDto(category) : null;
    }

    public async Task<IEnumerable<KnowledgeBaseCategoryDto>> GetCategoriesAsync(bool includeSubCategories = true)
    {
        var query = _context.Set<KnowledgeBaseCategory>()
            .Include(c => c.ParentCategory)
            .Include(c => c.Articles.Where(a => !a.IsDeleted && a.Status == "Published"))
            .Where(c => !c.IsDeleted && c.IsActive);

        if (includeSubCategories)
        {
            query = query.Include(c => c.SubCategories.Where(sc => !sc.IsDeleted && sc.IsActive));
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        var result = new List<KnowledgeBaseCategoryDto>();
        foreach (var category in categories)
        {
            result.Add(await MapToCategoryDto(category));
        }
        return result;
    }

    public async Task<KnowledgeBaseCategoryDto> UpdateCategoryAsync(Guid id, UpdateKnowledgeBaseCategoryDto dto)
    {
        var category = await _context.Set<KnowledgeBaseCategory>()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

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
        await _unitOfWork.SaveChangesAsync();

        return await MapToCategoryDto(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Set<KnowledgeBaseCategory>()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (category == null) return false;

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetArticleCountAsync(Guid? categoryId = null)
    {
        var query = _context.Set<KnowledgeBaseArticle>()
            .Where(a => !a.IsDeleted && a.Status == "Published");

        if (categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == categoryId.Value);
        }

        return await query.CountAsync();
    }

    public async Task<int> GetTotalViewsAsync(Guid? articleId = null)
    {
        var query = _context.Set<KnowledgeBaseView>()
            .Where(v => !v.IsDeleted);

        if (articleId.HasValue)
        {
            query = query.Where(v => v.ArticleId == articleId.Value);
        }

        return await query.CountAsync();
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

    private Task<KnowledgeBaseArticleDto> MapToArticleDto(KnowledgeBaseArticle article)
    {
        return Task.FromResult(new KnowledgeBaseArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Content = article.Content,
            Excerpt = article.Excerpt,
            CategoryId = article.CategoryId,
            CategoryName = article.Category?.Name,
            Status = article.Status,
            ViewCount = article.ViewCount,
            HelpfulCount = article.HelpfulCount,
            NotHelpfulCount = article.NotHelpfulCount,
            IsFeatured = article.IsFeatured,
            DisplayOrder = article.DisplayOrder,
            Tags = !string.IsNullOrEmpty(article.Tags) 
                ? article.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>(),
            AuthorId = article.AuthorId,
            AuthorName = article.Author != null ? $"{article.Author.FirstName} {article.Author.LastName}" : null,
            PublishedAt = article.PublishedAt,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        });
    }

    private async Task<KnowledgeBaseCategoryDto> MapToCategoryDto(KnowledgeBaseCategory category)
    {
        var articleCount = await _context.Set<KnowledgeBaseArticle>()
            .CountAsync(a => a.CategoryId == category.Id && !a.IsDeleted && a.Status == "Published");

        var subCategoriesList = new List<KnowledgeBaseCategoryDto>();
        if (category.SubCategories != null)
        {
            foreach (var sc in category.SubCategories.Where(sc => !sc.IsDeleted && sc.IsActive))
            {
                subCategoriesList.Add(await MapToCategoryDto(sc));
            }
        }

        return new KnowledgeBaseCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            IconUrl = category.IconUrl,
            ArticleCount = articleCount,
            SubCategories = subCategoriesList,
            CreatedAt = category.CreatedAt
        };
    }
}


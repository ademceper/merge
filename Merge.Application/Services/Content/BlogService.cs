using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text;
using Merge.Application.DTOs.Analytics;
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

public class BlogService : IBlogService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BlogService> _logger;
    private readonly ContentSettings _contentSettings;

    // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
    public BlogService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<BlogService> logger,
        IOptions<ContentSettings> contentSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _contentSettings = contentSettings.Value;
    }

    // Categories
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use CreateBlogCategoryCommand via MediatR instead")]
    public async Task<BlogCategoryDto> CreateCategoryAsync(object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateBlogCategoryDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        var slug = BlogCategory.GenerateSlug(dto.Name);
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        if (await _context.Set<BlogCategory>().AnyAsync(c => c.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var category = BlogCategory.Create(
            dto.Name,
            dto.Description,
            dto.ParentCategoryId,
            dto.ImageUrl,
            dto.DisplayOrder,
            dto.IsActive,
            slug);

        await _context.Set<BlogCategory>().AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToCategoryDtoWithAutoMapper(category, null);
    }

    public async Task<BlogCategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return category != null ? MapToCategoryDtoWithAutoMapper(category, null) : null;
    }

    public async Task<BlogCategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive, cancellationToken);

        return category != null ? MapToCategoryDtoWithAutoMapper(category, null) : null;
    }

    public async Task<IEnumerable<BlogCategoryDto>> GetAllCategoriesAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<BlogCategory> query = _context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory);

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Kategoriler genelde sınırlı ama güvenlik için limit ekle
        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Take(200) // ✅ Güvenlik: Maksimum 200 kategori
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Navigation property'den ID'leri al (zaten ToListAsync ile yüklenmiş, memory'de Select kabul edilebilir)
        var categoryIds = categories.Select(c => c.Id).ToList();
        var postCounts = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Where(p => categoryIds.Contains(p.CategoryId) && p.Status == ContentStatus.Published)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var result = new List<BlogCategoryDto>(categories.Count);
        foreach (var category in categories)
        {
            result.Add(MapToCategoryDtoWithAutoMapper(category, postCounts));
        }
        return result;
    }

    [Obsolete("Use UpdateBlogCategoryCommand via MediatR instead")]
    public async Task<bool> UpdateCategoryAsync(Guid id, object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateBlogCategoryDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<BlogCategory>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (!string.IsNullOrEmpty(dto.Name))
        {
            category.UpdateName(dto.Name);
        }
        if (dto.Description != null)
            category.UpdateDescription(dto.Description);
        if (dto.ParentCategoryId.HasValue)
            category.UpdateParentCategory(dto.ParentCategoryId);
        if (dto.ImageUrl != null)
            category.UpdateImageUrl(dto.ImageUrl);
        if (dto.DisplayOrder != 0)
            category.UpdateDisplayOrder(dto.DisplayOrder);
        if (dto.IsActive)
            category.Activate();
        else
            category.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<BlogCategory>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        category.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Posts
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    [Obsolete("Use CreateBlogPostCommand via MediatR instead")]
    public async Task<BlogPostDto> CreatePostAsync(Guid authorId, object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateBlogPostDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        _logger.LogInformation("Blog post olusturuluyor. AuthorId: {AuthorId}, CategoryId: {CategoryId}, Title: {Title}", 
            authorId, dto.CategoryId, dto.Title);

        try
        {
            // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
            var category = await _context.Set<BlogCategory>()
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.IsActive, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Blog post olusturma hatasi: Kategori bulunamadi. CategoryId: {CategoryId}", dto.CategoryId);
                throw new NotFoundException("Kategori", dto.CategoryId);
            }

            var slug = BlogPost.GenerateSlug(dto.Title);
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            if (await _context.Set<BlogPost>().AnyAsync(p => p.Slug == slug, cancellationToken))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            var readingTime = CalculateReadingTime(dto.Content);
            var statusEnum = Enum.TryParse<ContentStatus>(dto.Status, true, out var status) ? status : ContentStatus.Draft;
            var tags = dto.Tags != null ? string.Join(",", dto.Tags) : null;

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var post = BlogPost.Create(
                dto.CategoryId,
                authorId,
                dto.Title,
                dto.Excerpt,
                dto.Content,
                dto.FeaturedImageUrl,
                statusEnum,
                tags,
                dto.IsFeatured,
                dto.AllowComments,
                dto.MetaTitle,
                dto.MetaDescription,
                dto.MetaKeywords,
                dto.OgImageUrl,
                readingTime,
                slug); // Pass unique slug

            await _context.Set<BlogPost>().AddAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Blog post olusturuldu. PostId: {PostId}, Slug: {Slug}, AuthorId: {AuthorId}", 
                post.Id, post.Slug, authorId);

            return _mapper.Map<BlogPostDto>(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blog post olusturma hatasi. AuthorId: {AuthorId}, CategoryId: {CategoryId}, Title: {Title}", 
                authorId, dto.CategoryId, dto.Title);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    public async Task<BlogPostDto?> GetPostByIdAsync(Guid id, bool trackView = false, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (post == null) return null;

        if (trackView && post.Status == ContentStatus.Published)
        {
            await IncrementViewCountAsync(id, cancellationToken);
        }

        return _mapper.Map<BlogPostDto>(post);
    }

    public async Task<BlogPostDto?> GetPostBySlugAsync(string slug, bool trackView = false, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ContentStatus.Published, cancellationToken);

        if (post == null) return null;

        if (trackView)
        {
            await IncrementViewCountAsync(post.Id, cancellationToken);
        }

        return _mapper.Map<BlogPostDto>(post);
    }

    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PagedResult<BlogPostDto>> GetPostsByCategoryAsync(Guid categoryId, string? status = "Published", int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var query = _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrEmpty(status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<ContentStatus>(status, true, out var statusEnum))
            {
                query = query.Where(p => p.Status == statusEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var posts = await query
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = posts.Select(p => _mapper.Map<BlogPostDto>(p)).ToList();

        return new PagedResult<BlogPostDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
    public async Task<IEnumerable<BlogPostDto>> GetFeaturedPostsAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Configuration'dan max limit
        if (count > _contentSettings.MaxFeaturedPostsCount)
            count = _contentSettings.MaxFeaturedPostsCount;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var posts = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.IsFeatured && p.Status == ContentStatus.Published)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        var result = new List<BlogPostDto>();
        foreach (var post in posts)
        {
            result.Add(_mapper.Map<BlogPostDto>(post));
        }
        return result;
    }

    public async Task<IEnumerable<BlogPostDto>> GetRecentPostsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Configuration'dan max limit
        if (count > _contentSettings.MaxRecentPostsCount)
            count = _contentSettings.MaxRecentPostsCount;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var posts = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.Status == ContentStatus.Published)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        var result = new List<BlogPostDto>();
        foreach (var post in posts)
        {
            result.Add(_mapper.Map<BlogPostDto>(post));
        }
        return result;
    }

    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PagedResult<BlogPostDto>> SearchPostsAsync(string query, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var dbQuery = _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.Status == ContentStatus.Published &&
                       (p.Title.Contains(query) || p.Content.Contains(query) || p.Excerpt.Contains(query)));

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var posts = await dbQuery
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = posts.Select(p => _mapper.Map<BlogPostDto>(p)).ToList();

        return new PagedResult<BlogPostDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    [Obsolete("Use UpdateBlogPostCommand via MediatR instead")]
    public async Task<bool> UpdatePostAsync(Guid id, object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateBlogPostDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (post == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (!string.IsNullOrEmpty(dto.Title))
        {
            post.UpdateTitle(dto.Title);
        }
        if (dto.CategoryId != Guid.Empty)
            post.UpdateCategory(dto.CategoryId);
        if (!string.IsNullOrEmpty(dto.Excerpt))
            post.UpdateExcerpt(dto.Excerpt);
        if (!string.IsNullOrEmpty(dto.Content))
        {
            post.UpdateContent(dto.Content);
            post.UpdateReadingTime(CalculateReadingTime(dto.Content));
        }
        if (dto.FeaturedImageUrl != null)
            post.UpdateFeaturedImage(dto.FeaturedImageUrl);
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (!string.IsNullOrEmpty(dto.Status))
        {
            if (Enum.TryParse<ContentStatus>(dto.Status, true, out var newStatus))
            {
                post.UpdateStatus(newStatus);
            }
        }
        if (dto.Tags != null)
            post.UpdateTags(string.Join(",", dto.Tags));
        if (dto.IsFeatured)
            post.SetAsFeatured();
        else
            post.UnsetAsFeatured();
        post.UpdateAllowComments(dto.AllowComments);
        if (dto.MetaTitle != null || dto.MetaDescription != null || dto.MetaKeywords != null || dto.OgImageUrl != null)
            post.UpdateMetaInformation(dto.MetaTitle, dto.MetaDescription, dto.MetaKeywords, dto.OgImageUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeletePostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (post == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        post.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> PublishPostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (post == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        post.Publish();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (post == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        post.IncrementViewCount();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Comments
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use CreateBlogCommentCommand via MediatR instead")]
    public async Task<BlogCommentDto> CreateCommentAsync(Guid? userId, object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateBlogCommentDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == dto.BlogPostId && p.AllowComments, cancellationToken);

        if (post == null)
        {
            throw new NotFoundException("Blog yazısı", dto.BlogPostId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var autoApprove = userId.HasValue; // Auto-approve for logged-in users
        var comment = BlogComment.Create(
            dto.BlogPostId,
            dto.Content,
            userId,
            dto.ParentCommentId,
            dto.AuthorName,
            dto.AuthorEmail,
            autoApprove);

        await _context.Set<BlogComment>().AddAsync(comment, cancellationToken);
        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        post.IncrementCommentCount();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToCommentDtoWithAutoMapper(comment);
    }

    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PagedResult<BlogCommentDto>> GetPostCommentsAsync(Guid postId, bool? isApproved = true, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var query = _context.Set<BlogComment>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Replies)
            .Where(c => c.BlogPostId == postId && c.ParentCommentId == null);

        if (isApproved.HasValue)
        {
            query = query.Where(c => c.IsApproved == isApproved.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = comments.Select(c => MapToCommentDtoWithAutoMapper(c)).ToList();

        return new PagedResult<BlogCommentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ApproveCommentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comment = await _context.Set<BlogComment>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (comment == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        comment.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteCommentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comment = await _context.Set<BlogComment>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (comment == null) return false;

        var blogPostId = comment.BlogPostId;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        comment.MarkAsDeleted();
        
        // Decrement post comment count
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == blogPostId, cancellationToken);
        if (post != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            post.DecrementCommentCount();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Analytics
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<BlogAnalyticsDto> GetBlogAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = endDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var query = _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end);

        var totalPosts = await query.CountAsync(cancellationToken);
        var publishedPosts = await query.CountAsync(p => p.Status == ContentStatus.Published, cancellationToken);
        var draftPosts = await query.CountAsync(p => p.Status == ContentStatus.Draft, cancellationToken);
        var totalViews = await query.SumAsync(p => (long)p.ViewCount, cancellationToken);
        var totalComments = await query.SumAsync(p => (long)p.CommentCount, cancellationToken);

        // Database'de grouping yap
        var postsByCategory = await query
            .GroupBy(p => p.Category != null ? p.Category.Name : "Uncategorized")
            .Select(g => new { CategoryName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CategoryName, g => g.Count, cancellationToken);

        // ✅ PERFORMANCE: Database'de filtering, ordering ve projection yap
        var popularPosts = await query
            .Where(p => p.Status == ContentStatus.Published)
            .OrderByDescending(p => p.ViewCount)
            .Take(10)
            .Select(p => new PopularPostDto(
                p.Id,
                p.Title,
                p.ViewCount,
                p.CommentCount
            ))
            .ToListAsync(cancellationToken);

        return new BlogAnalyticsDto(
            totalPosts,
            publishedPosts,
            draftPosts,
            (int)totalViews,
            (int)totalComments,
            postsByCategory,
            popularPosts
        );
    }

    // Helper methods
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

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }

    // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
    private int CalculateReadingTime(string content)
    {
        // ✅ BOLUM 2.3: Configuration'dan al (hardcoded 200 YASAK)
        var wordCount = content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, wordCount / _contentSettings.AverageReadingSpeedWordsPerMinute);
    }

    private BlogCategoryDto MapToCategoryDtoWithAutoMapper(BlogCategory category, Dictionary<Guid, int>? postCounts = null)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<BlogCategoryDto>(category);
        
        // ✅ PERFORMANCE: postCount batch loading ile sağlanıyor (GetAllCategoriesAsync'de)
        // ✅ BOLUM 7.1: Records - with expression kullanımı (immutable DTOs)
        dto = dto with { PostCount = postCounts?.GetValueOrDefault(category.Id, 0) ?? 0 };

        // ✅ PERFORMANCE: Recursive mapping için SubCategories'leri manuel set et (AutoMapper recursive mapping'i desteklemiyor)
        if (category.SubCategories != null && category.SubCategories.Any())
        {
            var subCategories = new List<BlogCategoryDto>();
            foreach (var subCat in category.SubCategories.Where(sc => sc.IsActive).OrderBy(sc => sc.DisplayOrder))
            {
                subCategories.Add(MapToCategoryDtoWithAutoMapper(subCat, postCounts));
            }
            // ✅ BOLUM 7.1: Records - with expression kullanımı (immutable DTOs)
            dto = dto with { SubCategories = subCategories };
        }

        return dto;
    }


    private BlogCommentDto MapToCommentDtoWithAutoMapper(BlogComment comment)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<BlogCommentDto>(comment);
        
        // ✅ PERFORMANCE: Computed properties için manuel set et
        // ✅ BOLUM 7.1: Records - with expression kullanımı (immutable DTOs)
        dto = dto with { ReplyCount = comment.Replies?.Count(r => r.IsApproved) ?? 0 };
        
        // ✅ PERFORMANCE: Recursive mapping için Replies'leri manuel set et (AutoMapper recursive mapping'i desteklemiyor)
        // ✅ BOLUM 7.1: Records - with expression kullanımı (immutable DTOs)
        if (comment.Replies != null)
        {
            var replies = new List<BlogCommentDto>();
            foreach (var reply in comment.Replies.Where(r => r.IsApproved).OrderBy(r => r.CreatedAt))
            {
                replies.Add(MapToCommentDtoWithAutoMapper(reply));
            }
            dto = dto with { Replies = replies.AsReadOnly() };
        }

        return dto;
    }
}


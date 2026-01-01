using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Content;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;


namespace Merge.Application.Services.Content;

public class BlogService : IBlogService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BlogService> _logger;

    public BlogService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<BlogService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // Categories
    public async Task<BlogCategoryDto> CreateCategoryAsync(CreateBlogCategoryDto dto)
    {
        var slug = GenerateSlug(dto.Name);
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        if (await _context.Set<BlogCategory>().AnyAsync(c => c.Slug == slug))
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var category = new BlogCategory
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId,
            ImageUrl = dto.ImageUrl,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        await _context.Set<BlogCategory>().AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return MapToCategoryDtoWithAutoMapper(category, null);
    }

    public async Task<BlogCategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        return category != null ? MapToCategoryDtoWithAutoMapper(category, null) : null;
    }

    public async Task<BlogCategoryDto?> GetCategoryBySlugAsync(string slug)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

        return category != null ? MapToCategoryDtoWithAutoMapper(category, null) : null;
    }

    public async Task<IEnumerable<BlogCategoryDto>> GetAllCategoriesAsync(bool? isActive = null)
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

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch loading - tüm category'ler için postCount'ları tek query'de al
        var categoryIds = categories.Select(c => c.Id).ToList();
        var postCounts = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Where(p => categoryIds.Contains(p.CategoryId) && p.Status == "Published")
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

        var result = new List<BlogCategoryDto>();
        foreach (var category in categories)
        {
            result.Add(MapToCategoryDtoWithAutoMapper(category, postCounts));
        }
        return result;
    }

    public async Task<bool> UpdateCategoryAsync(Guid id, CreateBlogCategoryDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<BlogCategory>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;

        if (!string.IsNullOrEmpty(dto.Name))
        {
            category.Name = dto.Name;
            category.Slug = GenerateSlug(dto.Name);
        }
        if (dto.Description != null)
            category.Description = dto.Description;
        if (dto.ParentCategoryId.HasValue)
            category.ParentCategoryId = dto.ParentCategoryId;
        if (dto.ImageUrl != null)
            category.ImageUrl = dto.ImageUrl;
        if (dto.DisplayOrder != 0)
            category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;

        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<BlogCategory>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Posts
    public async Task<BlogPostDto> CreatePostAsync(Guid authorId, CreateBlogPostDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<BlogCategory>()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.IsActive);

        if (category == null)
        {
            throw new NotFoundException("Kategori", dto.CategoryId);
        }

        var slug = GenerateSlug(dto.Title);
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        if (await _context.Set<BlogPost>().AnyAsync(p => p.Slug == slug))
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var readingTime = CalculateReadingTime(dto.Content);

        var post = new BlogPost
        {
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            Title = dto.Title,
            Slug = slug,
            Excerpt = dto.Excerpt,
            Content = dto.Content,
            FeaturedImageUrl = dto.FeaturedImageUrl,
            Status = dto.Status,
            Tags = dto.Tags != null ? string.Join(",", dto.Tags) : null,
            IsFeatured = dto.IsFeatured,
            AllowComments = dto.AllowComments,
            MetaTitle = dto.MetaTitle,
            MetaDescription = dto.MetaDescription,
            MetaKeywords = dto.MetaKeywords,
            OgImageUrl = dto.OgImageUrl,
            ReadingTimeMinutes = readingTime,
            PublishedAt = dto.Status == "Published" ? DateTime.UtcNow : null
        };

        await _context.Set<BlogPost>().AddAsync(post);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BlogPostDto>(post);
    }

    public async Task<BlogPostDto?> GetPostByIdAsync(Guid id, bool trackView = false)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return null;

        if (trackView && post.Status == "Published")
        {
            await IncrementViewCountAsync(id);
        }

        return _mapper.Map<BlogPostDto>(post);
    }

    public async Task<BlogPostDto?> GetPostBySlugAsync(string slug, bool trackView = false)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == "Published");

        if (post == null) return null;

        if (trackView)
        {
            await IncrementViewCountAsync(post.Id);
        }

        return _mapper.Map<BlogPostDto>(post);
    }

    public async Task<IEnumerable<BlogPostDto>> GetPostsByCategoryAsync(Guid categoryId, string? status = "Published", int page = 1, int pageSize = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var query = _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status == status);
        }

        var posts = await query
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<BlogPostDto>();
        foreach (var post in posts)
        {
            result.Add(_mapper.Map<BlogPostDto>(post));
        }
        return result;
    }

    public async Task<IEnumerable<BlogPostDto>> GetFeaturedPostsAsync(int count = 5)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var posts = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.IsFeatured && p.Status == "Published")
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(count)
            .ToListAsync();

        var result = new List<BlogPostDto>();
        foreach (var post in posts)
        {
            result.Add(_mapper.Map<BlogPostDto>(post));
        }
        return result;
    }

    public async Task<IEnumerable<BlogPostDto>> GetRecentPostsAsync(int count = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var posts = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.Status == "Published")
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(count)
            .ToListAsync();

        var result = new List<BlogPostDto>();
        foreach (var post in posts)
        {
            result.Add(_mapper.Map<BlogPostDto>(post));
        }
        return result;
    }

    public async Task<IEnumerable<BlogPostDto>> SearchPostsAsync(string query, int page = 1, int pageSize = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var posts = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.Status == "Published" &&
                       (p.Title.Contains(query) || p.Content.Contains(query) || p.Excerpt.Contains(query)))
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<BlogPostDto>();
        foreach (var post in posts)
        {
            result.Add(_mapper.Map<BlogPostDto>(post));
        }
        return result;
    }

    public async Task<bool> UpdatePostAsync(Guid id, CreateBlogPostDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return false;

        if (!string.IsNullOrEmpty(dto.Title))
        {
            post.Title = dto.Title;
            post.Slug = GenerateSlug(dto.Title);
        }
        if (dto.CategoryId != Guid.Empty)
            post.CategoryId = dto.CategoryId;
        if (!string.IsNullOrEmpty(dto.Excerpt))
            post.Excerpt = dto.Excerpt;
        if (!string.IsNullOrEmpty(dto.Content))
        {
            post.Content = dto.Content;
            post.ReadingTimeMinutes = CalculateReadingTime(dto.Content);
        }
        if (dto.FeaturedImageUrl != null)
            post.FeaturedImageUrl = dto.FeaturedImageUrl;
        if (!string.IsNullOrEmpty(dto.Status))
        {
            post.Status = dto.Status;
            if (dto.Status == "Published" && !post.PublishedAt.HasValue)
            {
                post.PublishedAt = DateTime.UtcNow;
            }
        }
        if (dto.Tags != null)
            post.Tags = string.Join(",", dto.Tags);
        post.IsFeatured = dto.IsFeatured;
        post.AllowComments = dto.AllowComments;
        if (dto.MetaTitle != null)
            post.MetaTitle = dto.MetaTitle;
        if (dto.MetaDescription != null)
            post.MetaDescription = dto.MetaDescription;
        if (dto.MetaKeywords != null)
            post.MetaKeywords = dto.MetaKeywords;
        if (dto.OgImageUrl != null)
            post.OgImageUrl = dto.OgImageUrl;

        post.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return false;

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PublishPostAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return false;

        post.Status = "Published";
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IncrementViewCountAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return false;

        post.ViewCount++;
        post.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Comments
    public async Task<BlogCommentDto> CreateCommentAsync(Guid? userId, CreateBlogCommentDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == dto.BlogPostId && p.AllowComments);

        if (post == null)
        {
            throw new NotFoundException("Blog yazısı", dto.BlogPostId);
        }

        var comment = new BlogComment
        {
            BlogPostId = dto.BlogPostId,
            UserId = userId,
            ParentCommentId = dto.ParentCommentId,
            AuthorName = dto.AuthorName ?? string.Empty,
            AuthorEmail = dto.AuthorEmail ?? string.Empty,
            Content = dto.Content,
            IsApproved = userId.HasValue // Auto-approve for logged-in users
        };

        await _context.Set<BlogComment>().AddAsync(comment);
        post.CommentCount++;
        await _unitOfWork.SaveChangesAsync();

        return MapToCommentDtoWithAutoMapper(comment);
    }

    public async Task<IEnumerable<BlogCommentDto>> GetPostCommentsAsync(Guid postId, bool? isApproved = true)
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

        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var result = new List<BlogCommentDto>();
        foreach (var comment in comments)
        {
            result.Add(MapToCommentDtoWithAutoMapper(comment));
        }
        return result;
    }

    public async Task<bool> ApproveCommentAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comment = await _context.Set<BlogComment>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null) return false;

        comment.IsApproved = true;
        comment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCommentAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var comment = await _context.Set<BlogComment>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null) return false;

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        
        // Decrement post comment count
        var post = await _context.Set<BlogPost>()
            .FirstOrDefaultAsync(p => p.Id == comment.BlogPostId);
        if (post != null)
        {
            post.CommentCount = Math.Max(0, post.CommentCount - 1);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Analytics
    public async Task<BlogAnalyticsDto> GetBlogAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = endDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var query = _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end);

        var totalPosts = await query.CountAsync();
        var publishedPosts = await query.CountAsync(p => p.Status == "Published");
        var draftPosts = await query.CountAsync(p => p.Status == "Draft");
        var totalViews = await query.SumAsync(p => (long)p.ViewCount);
        var totalComments = await query.SumAsync(p => (long)p.CommentCount);

        // Database'de grouping yap
        var postsByCategory = await query
            .GroupBy(p => p.Category != null ? p.Category.Name : "Uncategorized")
            .Select(g => new { CategoryName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CategoryName, g => g.Count);

        // ✅ PERFORMANCE: Database'de filtering, ordering ve projection yap
        var popularPosts = await query
            .Where(p => p.Status == "Published")
            .OrderByDescending(p => p.ViewCount)
            .Take(10)
            .Select(p => new PopularPostDto
            {
                PostId = p.Id,
                Title = p.Title,
                ViewCount = p.ViewCount,
                CommentCount = p.CommentCount
            })
            .ToListAsync();

        return new BlogAnalyticsDto
        {
            TotalPosts = totalPosts,
            PublishedPosts = publishedPosts,
            DraftPosts = draftPosts,
            TotalViews = (int)totalViews,
            TotalComments = (int)totalComments,
            PostsByCategory = postsByCategory,
            PopularPosts = popularPosts
        };
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

    private int CalculateReadingTime(string content)
    {
        // Average reading speed: 200 words per minute
        var wordCount = content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, wordCount / 200);
    }

    private BlogCategoryDto MapToCategoryDtoWithAutoMapper(BlogCategory category, Dictionary<Guid, int>? postCounts = null)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<BlogCategoryDto>(category);
        
        // ✅ PERFORMANCE: postCount batch loading ile sağlanıyor (GetAllCategoriesAsync'de)
        dto.PostCount = postCounts?.GetValueOrDefault(category.Id, 0) ?? 0;

        // ✅ PERFORMANCE: Recursive mapping için SubCategories'leri manuel set et (AutoMapper recursive mapping'i desteklemiyor)
        if (category.SubCategories != null && category.SubCategories.Any())
        {
            var subCategories = new List<BlogCategoryDto>();
            foreach (var subCat in category.SubCategories.Where(sc => sc.IsActive).OrderBy(sc => sc.DisplayOrder))
            {
                subCategories.Add(MapToCategoryDtoWithAutoMapper(subCat, postCounts));
            }
            dto.SubCategories = subCategories;
        }

        return dto;
    }


    private BlogCommentDto MapToCommentDtoWithAutoMapper(BlogComment comment)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<BlogCommentDto>(comment);
        
        // ✅ PERFORMANCE: Computed properties için manuel set et
        dto.ReplyCount = comment.Replies?.Count(r => r.IsApproved) ?? 0;
        
        // ✅ PERFORMANCE: Recursive mapping için Replies'leri manuel set et (AutoMapper recursive mapping'i desteklemiyor)
        if (comment.Replies != null)
        {
            var replies = new List<BlogCommentDto>();
            foreach (var reply in comment.Replies.Where(r => r.IsApproved).OrderBy(r => r.CreatedAt))
            {
                replies.Add(MapToCommentDtoWithAutoMapper(reply));
            }
            dto.Replies = replies;
        }

        return dto;
    }
}


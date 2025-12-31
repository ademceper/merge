using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Interfaces.Content;

public interface IBlogService
{
    // Categories
    Task<BlogCategoryDto> CreateCategoryAsync(CreateBlogCategoryDto dto);
    Task<BlogCategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<BlogCategoryDto?> GetCategoryBySlugAsync(string slug);
    Task<IEnumerable<BlogCategoryDto>> GetAllCategoriesAsync(bool? isActive = null);
    Task<bool> UpdateCategoryAsync(Guid id, CreateBlogCategoryDto dto);
    Task<bool> DeleteCategoryAsync(Guid id);
    
    // Posts
    Task<BlogPostDto> CreatePostAsync(Guid authorId, CreateBlogPostDto dto);
    Task<BlogPostDto?> GetPostByIdAsync(Guid id, bool trackView = false);
    Task<BlogPostDto?> GetPostBySlugAsync(string slug, bool trackView = false);
    Task<IEnumerable<BlogPostDto>> GetPostsByCategoryAsync(Guid categoryId, string? status = "Published", int page = 1, int pageSize = 10);
    Task<IEnumerable<BlogPostDto>> GetFeaturedPostsAsync(int count = 5);
    Task<IEnumerable<BlogPostDto>> GetRecentPostsAsync(int count = 10);
    Task<IEnumerable<BlogPostDto>> SearchPostsAsync(string query, int page = 1, int pageSize = 10);
    Task<bool> UpdatePostAsync(Guid id, CreateBlogPostDto dto);
    Task<bool> DeletePostAsync(Guid id);
    Task<bool> PublishPostAsync(Guid id);
    Task<bool> IncrementViewCountAsync(Guid id);
    
    // Comments
    Task<BlogCommentDto> CreateCommentAsync(Guid? userId, CreateBlogCommentDto dto);
    Task<IEnumerable<BlogCommentDto>> GetPostCommentsAsync(Guid postId, bool? isApproved = true);
    Task<bool> ApproveCommentAsync(Guid id);
    Task<bool> DeleteCommentAsync(Guid id);
    
    // Analytics
    Task<BlogAnalyticsDto> GetBlogAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}



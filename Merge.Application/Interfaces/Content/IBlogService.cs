using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Content;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
public interface IBlogService
{
    // Categories
    [Obsolete("Use CreateBlogCategoryCommand via MediatR instead")]
    Task<BlogCategoryDto> CreateCategoryAsync(object dto, CancellationToken cancellationToken = default);
    Task<BlogCategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BlogCategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogCategoryDto>> GetAllCategoriesAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    [Obsolete("Use UpdateBlogCategoryCommand via MediatR instead")]
    Task<bool> UpdateCategoryAsync(Guid id, object dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    // Posts
    [Obsolete("Use CreateBlogPostCommand via MediatR instead")]
    Task<BlogPostDto> CreatePostAsync(Guid authorId, object dto, CancellationToken cancellationToken = default);
    Task<BlogPostDto?> GetPostByIdAsync(Guid id, bool trackView = false, CancellationToken cancellationToken = default);
    Task<BlogPostDto?> GetPostBySlugAsync(string slug, bool trackView = false, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<BlogPostDto>> GetPostsByCategoryAsync(Guid categoryId, string? status = "Published", int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogPostDto>> GetFeaturedPostsAsync(int count = 5, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogPostDto>> GetRecentPostsAsync(int count = 10, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<BlogPostDto>> SearchPostsAsync(string query, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    [Obsolete("Use UpdateBlogPostCommand via MediatR instead")]
    Task<bool> UpdatePostAsync(Guid id, object dto, CancellationToken cancellationToken = default);
    Task<bool> DeletePostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> PublishPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default);

    // Comments
    [Obsolete("Use CreateBlogCommentCommand via MediatR instead")]
    Task<BlogCommentDto> CreateCommentAsync(Guid? userId, object dto, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<BlogCommentDto>> GetPostCommentsAsync(Guid postId, bool? isApproved = true, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> ApproveCommentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteCommentAsync(Guid id, CancellationToken cancellationToken = default);

    // Analytics
    Task<BlogAnalyticsDto> GetBlogAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}



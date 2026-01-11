using Merge.Application.DTOs.Support;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Support;

public interface IKnowledgeBaseService
{
    // Articles
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<KnowledgeBaseArticleDto> CreateArticleAsync(CreateKnowledgeBaseArticleDto dto, Guid authorId, CancellationToken cancellationToken = default);
    Task<KnowledgeBaseArticleDto?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeBaseArticleDto?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<KnowledgeBaseArticleDto>> GetArticlesAsync(string? status = null, Guid? categoryId = null, bool featuredOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<KnowledgeBaseArticleDto>> SearchArticlesAsync(KnowledgeBaseSearchDto searchDto, CancellationToken cancellationToken = default);
    Task<KnowledgeBaseArticleDto> UpdateArticleAsync(Guid id, UpdateKnowledgeBaseArticleDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteArticleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> PublishArticleAsync(Guid id, CancellationToken cancellationToken = default);
    Task RecordArticleViewAsync(Guid articleId, Guid? userId = null, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);

    // Categories
    Task<KnowledgeBaseCategoryDto> CreateCategoryAsync(CreateKnowledgeBaseCategoryDto dto, CancellationToken cancellationToken = default);
    Task<KnowledgeBaseCategoryDto?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeBaseCategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<KnowledgeBaseCategoryDto>> GetCategoriesAsync(bool includeSubCategories = true, CancellationToken cancellationToken = default);
    Task<KnowledgeBaseCategoryDto> UpdateCategoryAsync(Guid id, UpdateKnowledgeBaseCategoryDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    // Stats
    Task<int> GetArticleCountAsync(Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<int> GetTotalViewsAsync(Guid? articleId = null, CancellationToken cancellationToken = default);
}


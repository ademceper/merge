using Merge.Application.DTOs.Support;
namespace Merge.Application.Interfaces.Support;

public interface IKnowledgeBaseService
{
    // Articles
    Task<KnowledgeBaseArticleDto> CreateArticleAsync(CreateKnowledgeBaseArticleDto dto, Guid authorId);
    Task<KnowledgeBaseArticleDto?> GetArticleAsync(Guid id);
    Task<KnowledgeBaseArticleDto?> GetArticleBySlugAsync(string slug);
    Task<IEnumerable<KnowledgeBaseArticleDto>> GetArticlesAsync(string? status = null, Guid? categoryId = null, bool featuredOnly = false, int page = 1, int pageSize = 20);
    Task<IEnumerable<KnowledgeBaseArticleDto>> SearchArticlesAsync(KnowledgeBaseSearchDto searchDto);
    Task<KnowledgeBaseArticleDto> UpdateArticleAsync(Guid id, UpdateKnowledgeBaseArticleDto dto);
    Task<bool> DeleteArticleAsync(Guid id);
    Task<bool> PublishArticleAsync(Guid id);
    Task RecordArticleViewAsync(Guid articleId, Guid? userId = null, string? ipAddress = null);

    // Categories
    Task<KnowledgeBaseCategoryDto> CreateCategoryAsync(CreateKnowledgeBaseCategoryDto dto);
    Task<KnowledgeBaseCategoryDto?> GetCategoryAsync(Guid id);
    Task<KnowledgeBaseCategoryDto?> GetCategoryBySlugAsync(string slug);
    Task<IEnumerable<KnowledgeBaseCategoryDto>> GetCategoriesAsync(bool includeSubCategories = true);
    Task<KnowledgeBaseCategoryDto> UpdateCategoryAsync(Guid id, UpdateKnowledgeBaseCategoryDto dto);
    Task<bool> DeleteCategoryAsync(Guid id);

    // Stats
    Task<int> GetArticleCountAsync(Guid? categoryId = null);
    Task<int> GetTotalViewsAsync(Guid? articleId = null);
}


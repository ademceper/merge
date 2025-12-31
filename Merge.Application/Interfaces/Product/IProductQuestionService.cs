using Merge.Application.DTOs.Product;

namespace Merge.Application.Interfaces.Product;

public interface IProductQuestionService
{
    // Questions
    Task<ProductQuestionDto> AskQuestionAsync(Guid userId, CreateProductQuestionDto dto);
    Task<ProductQuestionDto?> GetQuestionAsync(Guid questionId, Guid? userId = null);
    Task<IEnumerable<ProductQuestionDto>> GetProductQuestionsAsync(Guid productId, Guid? userId = null, int page = 1, int pageSize = 20);
    Task<IEnumerable<ProductQuestionDto>> GetUserQuestionsAsync(Guid userId);
    Task<bool> ApproveQuestionAsync(Guid questionId);
    Task<bool> DeleteQuestionAsync(Guid questionId);

    // Answers
    Task<ProductAnswerDto> AnswerQuestionAsync(Guid userId, CreateProductAnswerDto dto);
    Task<IEnumerable<ProductAnswerDto>> GetQuestionAnswersAsync(Guid questionId, Guid? userId = null);
    Task<bool> ApproveAnswerAsync(Guid answerId);
    Task<bool> DeleteAnswerAsync(Guid answerId);

    // Helpfulness
    Task MarkQuestionHelpfulAsync(Guid userId, Guid questionId);
    Task UnmarkQuestionHelpfulAsync(Guid userId, Guid questionId);
    Task MarkAnswerHelpfulAsync(Guid userId, Guid answerId);
    Task UnmarkAnswerHelpfulAsync(Guid userId, Guid answerId);

    // Stats
    Task<QAStatsDto> GetQAStatsAsync(Guid? productId = null);
    Task<IEnumerable<ProductQuestionDto>> GetUnansweredQuestionsAsync(Guid? productId = null, int limit = 20);
}

using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Interfaces.Product;

public interface IProductQuestionService
{
    // Questions
    Task<ProductQuestionDto> AskQuestionAsync(Guid userId, CreateProductQuestionDto dto, CancellationToken cancellationToken = default);
    Task<ProductQuestionDto?> GetQuestionAsync(Guid questionId, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductQuestionDto>> GetProductQuestionsAsync(Guid productId, Guid? userId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductQuestionDto>> GetUserQuestionsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> ApproveQuestionAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<bool> DeleteQuestionAsync(Guid questionId, CancellationToken cancellationToken = default);

    // Answers
    Task<ProductAnswerDto> AnswerQuestionAsync(Guid userId, CreateProductAnswerDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductAnswerDto>> GetQuestionAnswersAsync(Guid questionId, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<bool> ApproveAnswerAsync(Guid answerId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAnswerAsync(Guid answerId, CancellationToken cancellationToken = default);

    // Helpfulness
    Task MarkQuestionHelpfulAsync(Guid userId, Guid questionId, CancellationToken cancellationToken = default);
    Task UnmarkQuestionHelpfulAsync(Guid userId, Guid questionId, CancellationToken cancellationToken = default);
    Task MarkAnswerHelpfulAsync(Guid userId, Guid answerId, CancellationToken cancellationToken = default);
    Task UnmarkAnswerHelpfulAsync(Guid userId, Guid answerId, CancellationToken cancellationToken = default);

    // Stats
    Task<QAStatsDto> GetQAStatsAsync(Guid? productId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductQuestionDto>> GetUnansweredQuestionsAsync(Guid? productId = null, int limit = 20, CancellationToken cancellationToken = default);
}

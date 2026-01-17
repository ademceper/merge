using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.DeleteQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteQuestionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteQuestionCommandHandler> logger, ICacheService cache) : IRequestHandler<DeleteQuestionCommand, bool>
{

    private const string CACHE_KEY_QUESTION_BY_ID = "question_";
    private const string CACHE_KEY_PRODUCT_QUESTIONS = "product_questions_";
    private const string CACHE_KEY_USER_QUESTIONS = "user_questions_";
    private const string CACHE_KEY_UNANSWERED_QUESTIONS = "unanswered_questions_";
    private const string CACHE_KEY_QA_STATS = "qa_stats_";
    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";

    public async Task<bool> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting question. QuestionId: {QuestionId}, UserId: {UserId}", 
            request.QuestionId, request.UserId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var question = await context.Set<ProductQuestion>()
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

            if (question == null)
            {
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Kullanıcı sadece kendi sorularını silebilmeli
            if (question.UserId != request.UserId)
            {
                logger.LogWarning("Unauthorized attempt to delete question {QuestionId} by user {UserId}. Question belongs to {QuestionUserId}",
                    request.QuestionId, request.UserId, question.UserId);
                throw new BusinessException("Bu soruyu silme yetkiniz bulunmamaktadır.");
            }

            // Store IDs for cache invalidation
            var productId = question.ProductId;
            var userId = question.UserId;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            question.MarkAsDeleted();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            // Note: Paginated cache'ler (user_questions_*, product_questions_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (5 dakika TTL)
            await cache.RemoveAsync($"{CACHE_KEY_QUESTION_BY_ID}{request.QuestionId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_UNANSWERED_QUESTIONS}{productId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_QA_STATS}{productId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{request.QuestionId}", cancellationToken);

            logger.LogInformation("Question deleted successfully. QuestionId: {QuestionId}", request.QuestionId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting question. QuestionId: {QuestionId}", request.QuestionId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

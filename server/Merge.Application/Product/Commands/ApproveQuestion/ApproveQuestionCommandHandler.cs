using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.ApproveQuestion;

public class ApproveQuestionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ApproveQuestionCommandHandler> logger, ICacheService cache) : IRequestHandler<ApproveQuestionCommand, bool>
{

    private const string CACHE_KEY_QUESTION_BY_ID = "question_";
    private const string CACHE_KEY_PRODUCT_QUESTIONS = "product_questions_";
    private const string CACHE_KEY_UNANSWERED_QUESTIONS = "unanswered_questions_";
    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public async Task<bool> Handle(ApproveQuestionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Approving question. QuestionId: {QuestionId}", request.QuestionId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var question = await context.Set<ProductQuestion>()
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

            if (question == null)
            {
                return false;
            }

            question.Approve();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // Note: Paginated cache'ler (product_questions_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (5 dakika TTL)
            await cache.RemoveAsync($"{CACHE_KEY_QUESTION_BY_ID}{request.QuestionId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_UNANSWERED_QUESTIONS}{question.ProductId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_QA_STATS}{question.ProductId}", cancellationToken);

            logger.LogInformation("Question approved successfully. QuestionId: {QuestionId}", request.QuestionId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving question. QuestionId: {QuestionId}", request.QuestionId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

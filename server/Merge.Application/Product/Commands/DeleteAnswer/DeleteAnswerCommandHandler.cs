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

namespace Merge.Application.Product.Commands.DeleteAnswer;

public class DeleteAnswerCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteAnswerCommandHandler> logger, ICacheService cache) : IRequestHandler<DeleteAnswerCommand, bool>
{

    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";
    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public async Task<bool> Handle(DeleteAnswerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting answer. AnswerId: {AnswerId}, UserId: {UserId}", 
            request.AnswerId, request.UserId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var answer = await context.Set<ProductAnswer>()
                .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

            if (answer is null)
            {
                return false;
            }

            if (answer.UserId != request.UserId)
            {
                logger.LogWarning("Unauthorized attempt to delete answer {AnswerId} by user {UserId}. Answer belongs to {AnswerUserId}",
                    request.AnswerId, request.UserId, answer.UserId);
                throw new BusinessException("Bu cevabı silme yetkiniz bulunmamaktadır.");
            }

            // Store question ID for cache invalidation
            var questionId = answer.QuestionId;

            answer.MarkAsDeleted();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{questionId}", cancellationToken);

            // Get product ID for QA stats cache invalidation
            var question = await context.Set<ProductQuestion>()
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
            if (question is not null)
            {
                await cache.RemoveAsync($"{CACHE_KEY_QA_STATS}{question.ProductId}", cancellationToken);
            }

            logger.LogInformation("Answer deleted successfully. AnswerId: {AnswerId}", request.AnswerId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting answer. AnswerId: {AnswerId}", request.AnswerId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

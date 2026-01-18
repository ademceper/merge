using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.MarkQuestionHelpful;

public class MarkQuestionHelpfulCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkQuestionHelpfulCommandHandler> logger, ICacheService cache) : IRequestHandler<MarkQuestionHelpfulCommand>
{

    private const string CACHE_KEY_QUESTION_BY_ID = "question_";

    public async Task Handle(MarkQuestionHelpfulCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Marking question as helpful. UserId: {UserId}, QuestionId: {QuestionId}",
            request.UserId, request.QuestionId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await context.Set<QuestionHelpfulness>()
                .FirstOrDefaultAsync(qh => qh.QuestionId == request.QuestionId && qh.UserId == request.UserId, cancellationToken);

            if (existing is not null)
            {
                return; // Already marked
            }

            var helpfulness = QuestionHelpfulness.Create(
                request.QuestionId,
                request.UserId);

            await context.Set<QuestionHelpfulness>().AddAsync(helpfulness, cancellationToken);

            var question = await context.Set<ProductQuestion>()
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

            if (question is not null)
            {
                question.IncrementHelpfulCount();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_QUESTION_BY_ID}{request.QuestionId}", cancellationToken);

            logger.LogInformation("Question marked as helpful successfully. QuestionId: {QuestionId}", request.QuestionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking question as helpful. UserId: {UserId}, QuestionId: {QuestionId}",
                request.UserId, request.QuestionId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

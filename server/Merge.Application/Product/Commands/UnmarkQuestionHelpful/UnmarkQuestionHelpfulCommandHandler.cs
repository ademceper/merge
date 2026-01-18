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

namespace Merge.Application.Product.Commands.UnmarkQuestionHelpful;

public class UnmarkQuestionHelpfulCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UnmarkQuestionHelpfulCommandHandler> logger, ICacheService cache) : IRequestHandler<UnmarkQuestionHelpfulCommand>
{

    private const string CACHE_KEY_QUESTION_BY_ID = "question_";

    public async Task Handle(UnmarkQuestionHelpfulCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Unmarking question as helpful. UserId: {UserId}, QuestionId: {QuestionId}",
            request.UserId, request.QuestionId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var helpfulness = await context.Set<QuestionHelpfulness>()
                .FirstOrDefaultAsync(qh => qh.QuestionId == request.QuestionId && qh.UserId == request.UserId, cancellationToken);

            if (helpfulness is null)
            {
                return; // Not marked
            }

            context.Set<QuestionHelpfulness>().Remove(helpfulness);

            var question = await context.Set<ProductQuestion>()
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

            if (question is not null)
            {
                question.DecrementHelpfulCount();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_QUESTION_BY_ID}{request.QuestionId}", cancellationToken);

            logger.LogInformation("Question unmarked as helpful successfully. QuestionId: {QuestionId}", request.QuestionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unmarking question as helpful. UserId: {UserId}, QuestionId: {QuestionId}",
                request.UserId, request.QuestionId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

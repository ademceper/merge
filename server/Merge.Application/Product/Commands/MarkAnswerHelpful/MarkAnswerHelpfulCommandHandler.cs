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

namespace Merge.Application.Product.Commands.MarkAnswerHelpful;

public class MarkAnswerHelpfulCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkAnswerHelpfulCommandHandler> logger, ICacheService cache) : IRequestHandler<MarkAnswerHelpfulCommand>
{

    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";

    public async Task Handle(MarkAnswerHelpfulCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Marking answer as helpful. UserId: {UserId}, AnswerId: {AnswerId}",
            request.UserId, request.AnswerId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await context.Set<AnswerHelpfulness>()
                .FirstOrDefaultAsync(ah => ah.AnswerId == request.AnswerId && ah.UserId == request.UserId, cancellationToken);

            if (existing is not null)
            {
                return; // Already marked
            }

            var helpfulness = AnswerHelpfulness.Create(
                request.AnswerId,
                request.UserId);

            await context.Set<AnswerHelpfulness>().AddAsync(helpfulness, cancellationToken);

            var answer = await context.Set<ProductAnswer>()
                .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

            if (answer is not null)
            {
                answer.IncrementHelpfulCount();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            if (answer is not null)
            {
                await cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{answer.QuestionId}", cancellationToken);
            }

            logger.LogInformation("Answer marked as helpful successfully. AnswerId: {AnswerId}", request.AnswerId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking answer as helpful. UserId: {UserId}, AnswerId: {AnswerId}",
                request.UserId, request.AnswerId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

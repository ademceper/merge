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

namespace Merge.Application.Product.Commands.UnmarkAnswerHelpful;

public class UnmarkAnswerHelpfulCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UnmarkAnswerHelpfulCommandHandler> logger, ICacheService cache) : IRequestHandler<UnmarkAnswerHelpfulCommand>
{

    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";

    public async Task Handle(UnmarkAnswerHelpfulCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Unmarking answer as helpful. UserId: {UserId}, AnswerId: {AnswerId}",
            request.UserId, request.AnswerId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var helpfulness = await context.Set<AnswerHelpfulness>()
                .FirstOrDefaultAsync(ah => ah.AnswerId == request.AnswerId && ah.UserId == request.UserId, cancellationToken);

            if (helpfulness is null)
            {
                return; // Not marked
            }

            context.Set<AnswerHelpfulness>().Remove(helpfulness);

            var answer = await context.Set<ProductAnswer>()
                .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

            if (answer is not null)
            {
                answer.DecrementHelpfulCount();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            if (answer is not null)
            {
                await cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{answer.QuestionId}", cancellationToken);
            }

            logger.LogInformation("Answer unmarked as helpful successfully. AnswerId: {AnswerId}", request.AnswerId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unmarking answer as helpful. UserId: {UserId}, AnswerId: {AnswerId}",
                request.UserId, request.AnswerId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

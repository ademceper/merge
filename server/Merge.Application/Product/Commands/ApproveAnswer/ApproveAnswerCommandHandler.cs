using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.ApproveAnswer;

public class ApproveAnswerCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ApproveAnswerCommandHandler> logger, ICacheService cache) : IRequestHandler<ApproveAnswerCommand, bool>
{

    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";

    public async Task<bool> Handle(ApproveAnswerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Approving answer. AnswerId: {AnswerId}", request.AnswerId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var answer = await context.Set<ProductAnswer>()
                .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

            if (answer == null)
            {
                return false;
            }

            answer.Approve();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{answer.QuestionId}", cancellationToken);

            logger.LogInformation("Answer approved successfully. AnswerId: {AnswerId}", request.AnswerId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving answer. AnswerId: {AnswerId}", request.AnswerId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.AskQuestion;

public class AskQuestionCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<AskQuestionCommandHandler> logger,
    ICacheService cache,
    IMapper mapper) : IRequestHandler<AskQuestionCommand, ProductQuestionDto>
{

    private const string CACHE_KEY_USER_QUESTIONS = "user_questions_";
    private const string CACHE_KEY_PRODUCT_QUESTIONS = "product_questions_";
    private const string CACHE_KEY_UNANSWERED_QUESTIONS = "unanswered_questions_";
    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public async Task<ProductQuestionDto> Handle(AskQuestionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Asking product question. UserId: {UserId}, ProductId: {ProductId}",
            request.UserId, request.ProductId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            var question = ProductQuestion.Create(
                request.ProductId,
                request.UserId,
                request.Question);

            await context.Set<ProductQuestion>().AddAsync(question, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            question = await context.Set<ProductQuestion>()
                .AsNoTracking()
                .Include(q => q.Product)
                .Include(q => q.User)
                .Include(q => q.Answers.Where(a => a.IsApproved))
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(q => q.Id == question.Id, cancellationToken);

            logger.LogInformation("Product question asked successfully. QuestionId: {QuestionId}", question!.Id);

            // Note: Paginated cache'ler (user_questions_*, product_questions_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (5 dakika TTL)
            await cache.RemoveAsync($"{CACHE_KEY_UNANSWERED_QUESTIONS}{request.ProductId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_QA_STATS}{request.ProductId}", cancellationToken);

            var questionDto = mapper.Map<ProductQuestionDto>(question);
            questionDto = questionDto with { HasUserVoted = false };
            return questionDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error asking product question. UserId: {UserId}, ProductId: {ProductId}",
                request.UserId, request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

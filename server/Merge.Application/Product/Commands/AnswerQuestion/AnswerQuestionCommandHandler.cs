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

namespace Merge.Application.Product.Commands.AnswerQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AnswerQuestionCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<AnswerQuestionCommandHandler> logger,
    ICacheService cache,
    IMapper mapper) : IRequestHandler<AnswerQuestionCommand, ProductAnswerDto>
{

    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";
    private const string CACHE_KEY_PRODUCT_QUESTIONS = "product_questions_";
    private const string CACHE_KEY_UNANSWERED_QUESTIONS = "unanswered_questions_";
    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public async Task<ProductAnswerDto> Handle(AnswerQuestionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Answering product question. UserId: {UserId}, QuestionId: {QuestionId}",
            request.UserId, request.QuestionId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var question = await context.Set<ProductQuestion>()
                .Include(q => q.Product)
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

            if (question == null)
            {
                throw new NotFoundException("Soru", request.QuestionId);
            }

            // Check if user is seller
            var isSellerAnswer = question.Product.SellerId == request.UserId;

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var answer = ProductAnswer.Create(
                request.QuestionId,
                request.UserId,
                request.Answer,
                isSellerAnswer,
                false, // TODO: Check if user has purchased the product
                isSellerAnswer); // Auto-approve seller answers

            await context.Set<ProductAnswer>().AddAsync(answer, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            question.IncrementAnswerCount(isSellerAnswer);

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // Reload with includes
            answer = await context.Set<ProductAnswer>()
                .AsNoTracking()
                .Include(a => a.Question)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == answer.Id, cancellationToken);

            logger.LogInformation("Product question answered successfully. AnswerId: {AnswerId}", answer!.Id);

            // ✅ BOLUM 10.2: Cache invalidation
            // Note: Paginated cache'ler (product_questions_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (5 dakika TTL)
            await cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{request.QuestionId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_UNANSWERED_QUESTIONS}{question.ProductId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_QA_STATS}{question.ProductId}", cancellationToken);

            return mapper.Map<ProductAnswerDto>(answer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error answering product question. UserId: {UserId}, QuestionId: {QuestionId}",
                request.UserId, request.QuestionId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

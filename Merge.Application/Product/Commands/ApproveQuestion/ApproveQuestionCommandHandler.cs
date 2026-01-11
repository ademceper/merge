using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.ApproveQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ApproveQuestionCommandHandler : IRequestHandler<ApproveQuestionCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveQuestionCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_QUESTION_BY_ID = "question_";
    private const string CACHE_KEY_PRODUCT_QUESTIONS = "product_questions_";
    private const string CACHE_KEY_UNANSWERED_QUESTIONS = "unanswered_questions_";
    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public ApproveQuestionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ApproveQuestionCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(ApproveQuestionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving question. QuestionId: {QuestionId}", request.QuestionId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var question = await _context.Set<ProductQuestion>()
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

            if (question == null)
            {
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            question.Approve();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            // Note: Paginated cache'ler (product_questions_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (5 dakika TTL)
            await _cache.RemoveAsync($"{CACHE_KEY_QUESTION_BY_ID}{request.QuestionId}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_UNANSWERED_QUESTIONS}{question.ProductId}_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_QA_STATS}{question.ProductId}", cancellationToken);

            _logger.LogInformation("Question approved successfully. QuestionId: {QuestionId}", request.QuestionId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving question. QuestionId: {QuestionId}", request.QuestionId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.DeleteAnswer;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteAnswerCommandHandler : IRequestHandler<DeleteAnswerCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAnswerCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";
    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public DeleteAnswerCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAnswerCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteAnswerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting answer. AnswerId: {AnswerId}, UserId: {UserId}", 
            request.AnswerId, request.UserId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var answer = await _context.Set<ProductAnswer>()
                .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

            if (answer == null)
            {
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Kullanıcı sadece kendi cevaplarını silebilmeli
            if (answer.UserId != request.UserId)
            {
                _logger.LogWarning("Unauthorized attempt to delete answer {AnswerId} by user {UserId}. Answer belongs to {AnswerUserId}",
                    request.AnswerId, request.UserId, answer.UserId);
                throw new BusinessException("Bu cevabı silme yetkiniz bulunmamaktadır.");
            }

            // Store question ID for cache invalidation
            var questionId = answer.QuestionId;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            answer.MarkAsDeleted();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{questionId}", cancellationToken);

            // Get product ID for QA stats cache invalidation
            var question = await _context.Set<ProductQuestion>()
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
            if (question != null)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_QA_STATS}{question.ProductId}", cancellationToken);
            }

            _logger.LogInformation("Answer deleted successfully. AnswerId: {AnswerId}", request.AnswerId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting answer. AnswerId: {AnswerId}", request.AnswerId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

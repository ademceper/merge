using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.UnmarkQuestionHelpful;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UnmarkQuestionHelpfulCommandHandler : IRequestHandler<UnmarkQuestionHelpfulCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnmarkQuestionHelpfulCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_QUESTION_BY_ID = "question_";

    public UnmarkQuestionHelpfulCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UnmarkQuestionHelpfulCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task Handle(UnmarkQuestionHelpfulCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unmarking question as helpful. UserId: {UserId}, QuestionId: {QuestionId}",
            request.UserId, request.QuestionId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var helpfulness = await _context.Set<QuestionHelpfulness>()
                .FirstOrDefaultAsync(qh => qh.QuestionId == request.QuestionId && qh.UserId == request.UserId, cancellationToken);

            if (helpfulness == null)
            {
                return; // Not marked
            }

            _context.Set<QuestionHelpfulness>().Remove(helpfulness);

            var question = await _context.Set<ProductQuestion>()
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

            if (question != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                question.DecrementHelpfulCount();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation (helpful count değişti)
            await _cache.RemoveAsync($"{CACHE_KEY_QUESTION_BY_ID}{request.QuestionId}", cancellationToken);

            _logger.LogInformation("Question unmarked as helpful successfully. QuestionId: {QuestionId}", request.QuestionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unmarking question as helpful. UserId: {UserId}, QuestionId: {QuestionId}",
                request.UserId, request.QuestionId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

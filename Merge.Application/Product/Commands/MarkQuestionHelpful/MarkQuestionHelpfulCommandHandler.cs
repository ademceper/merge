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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class MarkQuestionHelpfulCommandHandler : IRequestHandler<MarkQuestionHelpfulCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkQuestionHelpfulCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_QUESTION_BY_ID = "question_";

    public MarkQuestionHelpfulCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<MarkQuestionHelpfulCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task Handle(MarkQuestionHelpfulCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking question as helpful. UserId: {UserId}, QuestionId: {QuestionId}",
            request.UserId, request.QuestionId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await _context.Set<QuestionHelpfulness>()
                .FirstOrDefaultAsync(qh => qh.QuestionId == request.QuestionId && qh.UserId == request.UserId, cancellationToken);

            if (existing != null)
            {
                return; // Already marked
            }

            var helpfulness = new QuestionHelpfulness
            {
                QuestionId = request.QuestionId,
                UserId = request.UserId
            };

            await _context.Set<QuestionHelpfulness>().AddAsync(helpfulness, cancellationToken);

            var question = await _context.Set<ProductQuestion>()
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

            if (question != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                question.IncrementHelpfulCount();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation (helpful count değişti)
            await _cache.RemoveAsync($"{CACHE_KEY_QUESTION_BY_ID}{request.QuestionId}", cancellationToken);

            _logger.LogInformation("Question marked as helpful successfully. QuestionId: {QuestionId}", request.QuestionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking question as helpful. UserId: {UserId}, QuestionId: {QuestionId}",
                request.UserId, request.QuestionId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

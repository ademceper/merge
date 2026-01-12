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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class MarkAnswerHelpfulCommandHandler : IRequestHandler<MarkAnswerHelpfulCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkAnswerHelpfulCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";

    public MarkAnswerHelpfulCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<MarkAnswerHelpfulCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task Handle(MarkAnswerHelpfulCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking answer as helpful. UserId: {UserId}, AnswerId: {AnswerId}",
            request.UserId, request.AnswerId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await _context.Set<AnswerHelpfulness>()
                .FirstOrDefaultAsync(ah => ah.AnswerId == request.AnswerId && ah.UserId == request.UserId, cancellationToken);

            if (existing != null)
            {
                return; // Already marked
            }

            var helpfulness = new AnswerHelpfulness
            {
                AnswerId = request.AnswerId,
                UserId = request.UserId
            };

            await _context.Set<AnswerHelpfulness>().AddAsync(helpfulness, cancellationToken);

            var answer = await _context.Set<ProductAnswer>()
                .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

            if (answer != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                answer.IncrementHelpfulCount();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation (helpful count değişti)
            if (answer != null)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{answer.QuestionId}", cancellationToken);
            }

            _logger.LogInformation("Answer marked as helpful successfully. AnswerId: {AnswerId}", request.AnswerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking answer as helpful. UserId: {UserId}, AnswerId: {AnswerId}",
                request.UserId, request.AnswerId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

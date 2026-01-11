using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.UnmarkAnswerHelpful;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UnmarkAnswerHelpfulCommandHandler : IRequestHandler<UnmarkAnswerHelpfulCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnmarkAnswerHelpfulCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";

    public UnmarkAnswerHelpfulCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UnmarkAnswerHelpfulCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task Handle(UnmarkAnswerHelpfulCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unmarking answer as helpful. UserId: {UserId}, AnswerId: {AnswerId}",
            request.UserId, request.AnswerId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var helpfulness = await _context.Set<AnswerHelpfulness>()
                .FirstOrDefaultAsync(ah => ah.AnswerId == request.AnswerId && ah.UserId == request.UserId, cancellationToken);

            if (helpfulness == null)
            {
                return; // Not marked
            }

            _context.Set<AnswerHelpfulness>().Remove(helpfulness);

            var answer = await _context.Set<ProductAnswer>()
                .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

            if (answer != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                answer.DecrementHelpfulCount();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation (helpful count değişti)
            if (answer != null)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{answer.QuestionId}", cancellationToken);
            }

            _logger.LogInformation("Answer unmarked as helpful successfully. AnswerId: {AnswerId}", request.AnswerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unmarking answer as helpful. UserId: {UserId}, AnswerId: {AnswerId}",
                request.UserId, request.AnswerId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

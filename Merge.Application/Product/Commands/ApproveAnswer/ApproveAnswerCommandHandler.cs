using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.ApproveAnswer;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ApproveAnswerCommandHandler : IRequestHandler<ApproveAnswerCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveAnswerCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";

    public ApproveAnswerCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ApproveAnswerCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(ApproveAnswerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving answer. AnswerId: {AnswerId}", request.AnswerId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var answer = await _context.Set<ProductAnswer>()
                .FirstOrDefaultAsync(a => a.Id == request.AnswerId, cancellationToken);

            if (answer == null)
            {
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            answer.Approve();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{answer.QuestionId}", cancellationToken);

            _logger.LogInformation("Answer approved successfully. AnswerId: {AnswerId}", request.AnswerId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving answer. AnswerId: {AnswerId}", request.AnswerId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

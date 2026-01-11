using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Product.Commands.AskQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AskQuestionCommandHandler : IRequestHandler<AskQuestionCommand, ProductQuestionDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<AskQuestionCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_USER_QUESTIONS = "user_questions_";
    private const string CACHE_KEY_PRODUCT_QUESTIONS = "product_questions_";
    private const string CACHE_KEY_UNANSWERED_QUESTIONS = "unanswered_questions_";
    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public AskQuestionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        ILogger<AskQuestionCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductQuestionDto> Handle(AskQuestionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Asking product question. UserId: {UserId}, ProductId: {ProductId}",
            request.UserId, request.ProductId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await _context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var question = ProductQuestion.Create(
                request.ProductId,
                request.UserId,
                request.Question);

            await _context.Set<ProductQuestion>().AddAsync(question, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            question = await _context.Set<ProductQuestion>()
                .AsNoTracking()
                .Include(q => q.Product)
                .Include(q => q.User)
                .Include(q => q.Answers.Where(a => a.IsApproved))
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(q => q.Id == question.Id, cancellationToken);

            _logger.LogInformation("Product question asked successfully. QuestionId: {QuestionId}", question!.Id);

            // ✅ BOLUM 10.2: Cache invalidation
            // Note: Paginated cache'ler (user_questions_*, product_questions_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (5 dakika TTL)
            await _cache.RemoveAsync($"{CACHE_KEY_UNANSWERED_QUESTIONS}{request.ProductId}_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_QA_STATS}{request.ProductId}", cancellationToken);

            // ✅ BOLUM 7.1.5: Records - with expression kullanımı (immutable record'lar için)
            var questionDto = _mapper.Map<ProductQuestionDto>(question);
            questionDto = questionDto with { HasUserVoted = false };
            return questionDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error asking product question. UserId: {UserId}, ProductId: {ProductId}",
                request.UserId, request.ProductId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

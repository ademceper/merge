using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Product.Commands.AnswerQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AnswerQuestionCommandHandler : IRequestHandler<AnswerQuestionCommand, ProductAnswerDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<AnswerQuestionCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";
    private const string CACHE_KEY_PRODUCT_QUESTIONS = "product_questions_";
    private const string CACHE_KEY_UNANSWERED_QUESTIONS = "unanswered_questions_";
    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public AnswerQuestionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        ILogger<AnswerQuestionCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductAnswerDto> Handle(AnswerQuestionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Answering product question. UserId: {UserId}, QuestionId: {QuestionId}",
            request.UserId, request.QuestionId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var question = await _context.Set<ProductQuestion>()
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

            await _context.Set<ProductAnswer>().AddAsync(answer, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            question.IncrementAnswerCount(isSellerAnswer);

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Reload with includes
            answer = await _context.Set<ProductAnswer>()
                .AsNoTracking()
                .Include(a => a.Question)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == answer.Id, cancellationToken);

            _logger.LogInformation("Product question answered successfully. AnswerId: {AnswerId}", answer!.Id);

            // ✅ BOLUM 10.2: Cache invalidation
            // Note: Paginated cache'ler (product_questions_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (5 dakika TTL)
            await _cache.RemoveAsync($"{CACHE_KEY_ANSWERS_BY_QUESTION}{request.QuestionId}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_UNANSWERED_QUESTIONS}{question.ProductId}_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_QA_STATS}{question.ProductId}", cancellationToken);

            return _mapper.Map<ProductAnswerDto>(answer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering product question. UserId: {UserId}, QuestionId: {QuestionId}",
                request.UserId, request.QuestionId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

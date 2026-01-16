using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetQuestion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetQuestionQueryHandler : IRequestHandler<GetQuestionQuery, ProductQuestionDto?>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetQuestionQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_QUESTION_BY_ID = "question_";

    public GetQuestionQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetQuestionQueryHandler> logger,
        ICacheService cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<ProductQuestionDto?> Handle(GetQuestionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching question by Id: {QuestionId}, UserId: {UserId}", request.QuestionId, request.UserId);

        // ✅ BOLUM 10.1: Cache-Aside Pattern (UserId-specific cache key for user-specific data)
        var cacheKey = $"{CACHE_KEY_QUESTION_BY_ID}{request.QuestionId}";
        // Note: UserId-specific data (HasUserVoted) is not cached, only question data
        var cachedQuestion = await _cache.GetAsync<ProductQuestionDto>(cacheKey, cancellationToken);
        if (cachedQuestion != null && !request.UserId.HasValue)
        {
            _logger.LogInformation("Question retrieved from cache. QuestionId: {QuestionId}", request.QuestionId);
            return cachedQuestion;
        }

        _logger.LogInformation("Cache miss for question. QuestionId: {QuestionId}", request.QuestionId);

        var question = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

        if (question == null)
        {
            _logger.LogWarning("Question not found with Id: {QuestionId}", request.QuestionId);
            return null;
        }

        var hasUserVoted = request.UserId.HasValue
            ? await _context.Set<QuestionHelpfulness>()
                .AsNoTracking()
                .AnyAsync(qh => qh.QuestionId == question.Id && qh.UserId == request.UserId.Value, cancellationToken)
            : false;

        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (immutable record'lar için)
        var questionDto = _mapper.Map<ProductQuestionDto>(question);
        questionDto = questionDto with { HasUserVoted = hasUserVoted };

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz (sadece UserId yoksa)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        if (!request.UserId.HasValue)
        {
            await _cache.SetAsync(cacheKey, questionDto, TimeSpan.FromMinutes(_cacheSettings.QuestionCacheExpirationMinutes), cancellationToken);
        }

        _logger.LogInformation("Question retrieved successfully. QuestionId: {QuestionId}", request.QuestionId);

        return questionDto;
    }
}

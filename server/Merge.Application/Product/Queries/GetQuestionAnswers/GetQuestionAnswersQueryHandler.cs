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

namespace Merge.Application.Product.Queries.GetQuestionAnswers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetQuestionAnswersQueryHandler : IRequestHandler<GetQuestionAnswersQuery, IEnumerable<ProductAnswerDto>>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetQuestionAnswersQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";

    public GetQuestionAnswersQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetQuestionAnswersQueryHandler> logger,
        ICacheService cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<IEnumerable<ProductAnswerDto>> Handle(GetQuestionAnswersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching answers for question. QuestionId: {QuestionId}, UserId: {UserId}", 
            request.QuestionId, request.UserId);

        // ✅ BOLUM 10.1: Cache-Aside Pattern (UserId-specific cache key for user-specific data)
        var cacheKey = $"{CACHE_KEY_ANSWERS_BY_QUESTION}{request.QuestionId}";
        // Note: UserId-specific data (HasUserVoted) is not cached, only answer data
        var cachedAnswers = await _cache.GetAsync<IEnumerable<ProductAnswerDto>>(cacheKey, cancellationToken);
        if (cachedAnswers != null && !request.UserId.HasValue)
        {
            _logger.LogInformation("Answers retrieved from cache. QuestionId: {QuestionId}", request.QuestionId);
            return cachedAnswers;
        }

        _logger.LogInformation("Cache miss for answers. QuestionId: {QuestionId}", request.QuestionId);

        // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
        var answersQuery = _context.Set<ProductAnswer>()
            .AsNoTracking()
            .Where(a => a.QuestionId == request.QuestionId && a.IsApproved)
            .OrderByDescending(a => a.IsSellerAnswer)
            .ThenByDescending(a => a.HelpfulCount)
            .ThenByDescending(a => a.CreatedAt);

        var answers = await answersQuery
            .Include(a => a.User)
            .ToListAsync(cancellationToken);

        var answerIdsSubquery = from a in answersQuery select a.Id;
        var userVotes = request.UserId.HasValue
            ? await _context.Set<AnswerHelpfulness>()
                .AsNoTracking()
                .Where(ah => answerIdsSubquery.Contains(ah.AnswerId) && ah.UserId == request.UserId.Value)
                .ToDictionaryAsync(ah => ah.AnswerId, cancellationToken)
            : new Dictionary<Guid, AnswerHelpfulness>();

        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (immutable record'lar için)
        var dtos = new List<ProductAnswerDto>(answers.Count);
        foreach (var answer in answers)
        {
            var dto = _mapper.Map<ProductAnswerDto>(answer);
            dto = dto with { HasUserVoted = userVotes.ContainsKey(answer.Id) };
            dtos.Add(dto);
        }

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz (sadece UserId yoksa)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        if (!request.UserId.HasValue)
        {
            await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(_cacheSettings.AnswerCacheExpirationMinutes), cancellationToken);
        }

        _logger.LogInformation("Retrieved answers for question. QuestionId: {QuestionId}, Count: {Count}", 
            request.QuestionId, answers.Count);

        return dtos;
    }
}

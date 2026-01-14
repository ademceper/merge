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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetQAStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetQAStatsQueryHandler : IRequestHandler<GetQAStatsQuery, QAStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetQAStatsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public GetQAStatsQueryHandler(
        IDbContext context,
        ILogger<GetQAStatsQueryHandler> logger,
        ICacheService cache,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<QAStatsDto> Handle(GetQAStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching QA stats. ProductId: {ProductId}", request.ProductId);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = $"{CACHE_KEY_QA_STATS}{request.ProductId ?? Guid.Empty}";
        var cachedStats = await _cache.GetAsync<QAStatsDto>(cacheKey, cancellationToken);
        if (cachedStats != null)
        {
            _logger.LogInformation("QA stats retrieved from cache. ProductId: {ProductId}", request.ProductId);
            return cachedStats;
        }

        _logger.LogInformation("Cache miss for QA stats. Fetching from database.");

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var query = _context.Set<ProductQuestion>().AsNoTracking();

        if (request.ProductId.HasValue)
        {
            query = query.Where(q => q.ProductId == request.ProductId.Value);
        }

        var totalQuestions = await query.CountAsync(cancellationToken);
        var approvedQuestions = await query.CountAsync(q => q.IsApproved, cancellationToken);
        var unansweredQuestions = await query.CountAsync(q => q.AnswerCount == 0, cancellationToken);
        var totalAnswers = await _context.Set<ProductAnswer>()
            .AsNoTracking()
            .CountAsync(a => request.ProductId == null || a.Question.ProductId == request.ProductId.Value, cancellationToken);

        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var stats = new QAStatsDto(
            TotalQuestions: totalQuestions,
            TotalAnswers: totalAnswers,
            UnansweredQuestions: unansweredQuestions,
            QuestionsWithSellerAnswer: 0, // TODO: Calculate from database
            AverageAnswersPerQuestion: totalQuestions > 0 ? (decimal)totalAnswers / totalQuestions : 0,
            RecentQuestions: Array.Empty<ProductQuestionDto>(),
            MostHelpfulQuestions: Array.Empty<ProductQuestionDto>()
        );

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        await _cache.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(_cacheSettings.QAStatsCacheExpirationMinutes), cancellationToken);

        _logger.LogInformation("Retrieved QA stats. ProductId: {ProductId}, TotalQuestions: {TotalQuestions}, TotalAnswers: {TotalAnswers}",
            request.ProductId, totalQuestions, totalAnswers);

        return stats;
    }
}

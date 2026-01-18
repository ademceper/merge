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

public class GetQAStatsQueryHandler(IDbContext context, ILogger<GetQAStatsQueryHandler> logger, ICacheService cache, IOptions<CacheSettings> cacheSettings) : IRequestHandler<GetQAStatsQuery, QAStatsDto>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;


    private const string CACHE_KEY_QA_STATS = "qa_stats_";

    public async Task<QAStatsDto> Handle(GetQAStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching QA stats. ProductId: {ProductId}", request.ProductId);

        var cacheKey = $"{CACHE_KEY_QA_STATS}{request.ProductId ?? Guid.Empty}";
        var cachedStats = await cache.GetAsync<QAStatsDto>(cacheKey, cancellationToken);
        if (cachedStats != null)
        {
            logger.LogInformation("QA stats retrieved from cache. ProductId: {ProductId}", request.ProductId);
            return cachedStats;
        }

        logger.LogInformation("Cache miss for QA stats. Fetching from database.");

        var query = context.Set<ProductQuestion>().AsNoTracking();

        if (request.ProductId.HasValue)
        {
            query = query.Where(q => q.ProductId == request.ProductId.Value);
        }

        var totalQuestions = await query.CountAsync(cancellationToken);
        var approvedQuestions = await query.CountAsync(q => q.IsApproved, cancellationToken);
        var unansweredQuestions = await query.CountAsync(q => q.AnswerCount == 0, cancellationToken);
        var totalAnswers = await context.Set<ProductAnswer>()
            .AsNoTracking()
            .CountAsync(a => request.ProductId == null || a.Question.ProductId == request.ProductId.Value, cancellationToken);

        var stats = new QAStatsDto(
            TotalQuestions: totalQuestions,
            TotalAnswers: totalAnswers,
            UnansweredQuestions: unansweredQuestions,
            QuestionsWithSellerAnswer: 0, // TODO: Calculate from database
            AverageAnswersPerQuestion: totalQuestions > 0 ? (decimal)totalAnswers / totalQuestions : 0,
            RecentQuestions: Array.Empty<ProductQuestionDto>(),
            MostHelpfulQuestions: Array.Empty<ProductQuestionDto>()
        );

        await cache.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(cacheConfig.QAStatsCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Retrieved QA stats. ProductId: {ProductId}, TotalQuestions: {TotalQuestions}, TotalAnswers: {TotalAnswers}",
            request.ProductId, totalQuestions, totalAnswers);

        return stats;
    }
}

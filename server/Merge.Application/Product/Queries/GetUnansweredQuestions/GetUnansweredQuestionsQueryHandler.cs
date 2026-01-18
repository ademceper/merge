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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using AutoMapper;

namespace Merge.Application.Product.Queries.GetUnansweredQuestions;

public class GetUnansweredQuestionsQueryHandler(
    IDbContext context,
    ILogger<GetUnansweredQuestionsQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetUnansweredQuestionsQuery, IEnumerable<ProductQuestionDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_UNANSWERED_QUESTIONS = "unanswered_questions_";

    public async Task<IEnumerable<ProductQuestionDto>> Handle(GetUnansweredQuestionsQuery request, CancellationToken cancellationToken)
    {
        var limit = request.Limit > paginationConfig.MaxPageSize
            ? paginationConfig.MaxPageSize
            : request.Limit;
        if (limit < 1) limit = paginationConfig.DefaultPageSize;

        logger.LogInformation("Fetching unanswered questions. ProductId: {ProductId}, Limit: {Limit}", 
            request.ProductId, limit);

        var cacheKey = $"{CACHE_KEY_UNANSWERED_QUESTIONS}{request.ProductId ?? Guid.Empty}_{limit}";
        var cachedQuestions = await cache.GetAsync<IEnumerable<ProductQuestionDto>>(cacheKey, cancellationToken);
        if (cachedQuestions is not null)
        {
            logger.LogInformation("Unanswered questions retrieved from cache. ProductId: {ProductId}, Limit: {Limit}",
                request.ProductId, limit);
            return cachedQuestions;
        }

        logger.LogInformation("Cache miss for unanswered questions. Fetching from database.");

        var query = context.Set<ProductQuestion>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Where(q => q.AnswerCount == 0 && q.IsApproved);

        if (request.ProductId.HasValue)
        {
            query = query.Where(q => q.ProductId == request.ProductId.Value);
        }

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var questionDtos = questions.Select(q => mapper.Map<ProductQuestionDto>(q)).ToList();

        await cache.SetAsync(cacheKey, questionDtos, TimeSpan.FromMinutes(cacheConfig.UnansweredQuestionsCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Retrieved unanswered questions. ProductId: {ProductId}, Count: {Count}, Limit: {Limit}",
            request.ProductId, questions.Count, limit);

        return questionDtos;
    }
}

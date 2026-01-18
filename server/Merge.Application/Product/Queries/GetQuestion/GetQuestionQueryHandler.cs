using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
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

public class GetQuestionQueryHandler(
    IDbContext context,
    ILogger<GetQuestionQueryHandler> logger,
    ICacheService cache,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetQuestionQuery, ProductQuestionDto?>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_QUESTION_BY_ID = "question_";

    public async Task<ProductQuestionDto?> Handle(GetQuestionQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching question by Id: {QuestionId}, UserId: {UserId}", request.QuestionId, request.UserId);

        var cacheKey = $"{CACHE_KEY_QUESTION_BY_ID}{request.QuestionId}";
        // Note: UserId-specific data (HasUserVoted) is not cached, only question data
        var cachedQuestion = await cache.GetAsync<ProductQuestionDto>(cacheKey, cancellationToken);
        if (cachedQuestion != null && !request.UserId.HasValue)
        {
            logger.LogInformation("Question retrieved from cache. QuestionId: {QuestionId}", request.QuestionId);
            return cachedQuestion;
        }

        logger.LogInformation("Cache miss for question. QuestionId: {QuestionId}", request.QuestionId);

        var question = await context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

        if (question == null)
        {
            logger.LogWarning("Question not found with Id: {QuestionId}", request.QuestionId);
            return null;
        }

        var hasUserVoted = request.UserId.HasValue
            ? await context.Set<QuestionHelpfulness>()
                .AsNoTracking()
                .AnyAsync(qh => qh.QuestionId == question.Id && qh.UserId == request.UserId.Value, cancellationToken)
            : false;

        var questionDto = mapper.Map<ProductQuestionDto>(question);
        questionDto = questionDto with { HasUserVoted = hasUserVoted };

        if (!request.UserId.HasValue)
        {
            await cache.SetAsync(cacheKey, questionDto, TimeSpan.FromMinutes(cacheConfig.QuestionCacheExpirationMinutes), cancellationToken);
        }

        logger.LogInformation("Question retrieved successfully. QuestionId: {QuestionId}", request.QuestionId);

        return questionDto;
    }
}

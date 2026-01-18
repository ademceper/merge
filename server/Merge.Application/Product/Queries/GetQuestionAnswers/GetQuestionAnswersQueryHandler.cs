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

namespace Merge.Application.Product.Queries.GetQuestionAnswers;

public class GetQuestionAnswersQueryHandler(
    IDbContext context,
    ILogger<GetQuestionAnswersQueryHandler> logger,
    ICacheService cache,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetQuestionAnswersQuery, IEnumerable<ProductAnswerDto>>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_ANSWERS_BY_QUESTION = "answers_by_question_";

    public async Task<IEnumerable<ProductAnswerDto>> Handle(GetQuestionAnswersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching answers for question. QuestionId: {QuestionId}, UserId: {UserId}", 
            request.QuestionId, request.UserId);

        var cacheKey = $"{CACHE_KEY_ANSWERS_BY_QUESTION}{request.QuestionId}";
        // Note: UserId-specific data (HasUserVoted) is not cached, only answer data
        var cachedAnswers = await cache.GetAsync<IEnumerable<ProductAnswerDto>>(cacheKey, cancellationToken);
        if (cachedAnswers != null && !request.UserId.HasValue)
        {
            logger.LogInformation("Answers retrieved from cache. QuestionId: {QuestionId}", request.QuestionId);
            return cachedAnswers;
        }

        logger.LogInformation("Cache miss for answers. QuestionId: {QuestionId}", request.QuestionId);

        var answersQuery = context.Set<ProductAnswer>()
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
            ? await context.Set<AnswerHelpfulness>()
                .AsNoTracking()
                .Where(ah => answerIdsSubquery.Contains(ah.AnswerId) && ah.UserId == request.UserId.Value)
                .ToDictionaryAsync(ah => ah.AnswerId, cancellationToken)
            : new Dictionary<Guid, AnswerHelpfulness>();

        var dtos = new List<ProductAnswerDto>(answers.Count);
        foreach (var answer in answers)
        {
            var dto = mapper.Map<ProductAnswerDto>(answer);
            dto = dto with { HasUserVoted = userVotes.ContainsKey(answer.Id) };
            dtos.Add(dto);
        }

        if (!request.UserId.HasValue)
        {
            await cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(cacheConfig.AnswerCacheExpirationMinutes), cancellationToken);
        }

        logger.LogInformation("Retrieved answers for question. QuestionId: {QuestionId}, Count: {Count}", 
            request.QuestionId, answers.Count);

        return dtos;
    }
}

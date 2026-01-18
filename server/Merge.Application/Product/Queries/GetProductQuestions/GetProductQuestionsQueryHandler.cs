using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetProductQuestions;

public class GetProductQuestionsQueryHandler(
    IDbContext context,
    ILogger<GetProductQuestionsQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetProductQuestionsQuery, PagedResult<ProductQuestionDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_PRODUCT_QUESTIONS = "product_questions_";

    public async Task<PagedResult<ProductQuestionDto>> Handle(GetProductQuestionsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching product questions. ProductId: {ProductId}, UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.ProductId, request.UserId, request.Page, request.PageSize);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > paginationConfig.MaxPageSize
            ? paginationConfig.MaxPageSize
            : request.PageSize;

        // Cache key includes UserId for user-specific data (HasUserVoted)
        var cacheKey = $"{CACHE_KEY_PRODUCT_QUESTIONS}{request.ProductId}_{request.UserId ?? Guid.Empty}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for product questions. Fetching from database.");

                var query = context.Set<ProductQuestion>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.Product)
                    .Include(q => q.User)
                    .Include(q => q.Answers.Where(a => a.IsApproved))
                        .ThenInclude(a => a.User)
                    .Where(q => q.ProductId == request.ProductId && q.IsApproved);

                var totalCount = await query.CountAsync(cancellationToken);

                var paginatedQuestionsQuery = query
                    .OrderByDescending(q => q.HasSellerAnswer)
                    .ThenByDescending(q => q.HelpfulCount)
                    .ThenByDescending(q => q.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var questions = await paginatedQuestionsQuery.ToListAsync(cancellationToken);

                var questionIdsSubquery = from q in paginatedQuestionsQuery select q.Id;
                var userVotes = request.UserId.HasValue
                    ? await context.Set<QuestionHelpfulness>()
                        .AsNoTracking()
                        .Where(qh => questionIdsSubquery.Contains(qh.QuestionId) && qh.UserId == request.UserId.Value)
                        .ToDictionaryAsync(qh => qh.QuestionId, cancellationToken)
                    : new Dictionary<Guid, QuestionHelpfulness>();

                var dtos = new List<ProductQuestionDto>(questions.Count);
                foreach (var question in questions)
                {
                    var dto = mapper.Map<ProductQuestionDto>(question);
                    dto = dto with { HasUserVoted = userVotes.ContainsKey(question.Id) };
                    dtos.Add(dto);
                }

                return new PagedResult<ProductQuestionDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            TimeSpan.FromMinutes(cacheConfig.ProductQuestionsCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }
}

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

namespace Merge.Application.Product.Queries.GetUserQuestions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetUserQuestionsQueryHandler(
    IDbContext context,
    ILogger<GetUserQuestionsQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetUserQuestionsQuery, PagedResult<ProductQuestionDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_USER_QUESTIONS = "user_questions_";

    public async Task<PagedResult<ProductQuestionDto>> Handle(GetUserQuestionsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching user questions. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > paginationConfig.MaxPageSize
            ? paginationConfig.MaxPageSize
            : request.PageSize;

        var cacheKey = $"{CACHE_KEY_USER_QUESTIONS}{request.UserId}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for user questions. Fetching from database.");

                // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes with ThenInclude)
                var query = context.Set<ProductQuestion>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.Product)
                    .Include(q => q.User)
                    .Include(q => q.Answers.Where(a => a.IsApproved))
                        .ThenInclude(a => a.User)
                    .Where(q => q.UserId == request.UserId);

                var totalCount = await query.CountAsync(cancellationToken);

                var paginatedQuestionsQuery = query
                    .OrderByDescending(q => q.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                var questions = await paginatedQuestionsQuery.ToListAsync(cancellationToken);

                // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
                var questionIdsSubquery = from q in paginatedQuestionsQuery select q.Id;
                var userVotes = await context.Set<QuestionHelpfulness>()
                    .AsNoTracking()
                    .Where(qh => questionIdsSubquery.Contains(qh.QuestionId) && qh.UserId == request.UserId)
                    .ToDictionaryAsync(qh => qh.QuestionId, cancellationToken);

                // ✅ BOLUM 7.1.5: Records - with expression kullanımı (immutable record'lar için)
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
            TimeSpan.FromMinutes(cacheConfig.UserQuestionsCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }
}

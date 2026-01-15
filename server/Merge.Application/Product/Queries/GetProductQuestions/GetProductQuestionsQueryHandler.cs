using MediatR;
using Microsoft.EntityFrameworkCore;
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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetProductQuestionsQueryHandler : IRequestHandler<GetProductQuestionsQuery, PagedResult<ProductQuestionDto>>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetProductQuestionsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private readonly CacheSettings _cacheSettings;
    private const string CACHE_KEY_PRODUCT_QUESTIONS = "product_questions_";

    public GetProductQuestionsQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetProductQuestionsQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings,
        IOptions<CacheSettings> cacheSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<PagedResult<ProductQuestionDto>> Handle(GetProductQuestionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product questions. ProductId: {ProductId}, UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.ProductId, request.UserId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize
            ? _paginationSettings.MaxPageSize
            : request.PageSize;

        // Cache key includes UserId for user-specific data (HasUserVoted)
        var cacheKey = $"{CACHE_KEY_PRODUCT_QUESTIONS}{request.ProductId}_{request.UserId ?? Guid.Empty}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for product questions. Fetching from database.");

                // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes with ThenInclude)
                var query = _context.Set<ProductQuestion>()
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

                // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
                var questionIdsSubquery = from q in paginatedQuestionsQuery select q.Id;
                var userVotes = request.UserId.HasValue
                    ? await _context.Set<QuestionHelpfulness>()
                        .AsNoTracking()
                        .Where(qh => questionIdsSubquery.Contains(qh.QuestionId) && qh.UserId == request.UserId.Value)
                        .ToDictionaryAsync(qh => qh.QuestionId, cancellationToken)
                    : new Dictionary<Guid, QuestionHelpfulness>();

                // ✅ BOLUM 7.1.5: Records - with expression kullanımı (immutable record'lar için)
                var dtos = new List<ProductQuestionDto>(questions.Count);
                foreach (var question in questions)
                {
                    var dto = _mapper.Map<ProductQuestionDto>(question);
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
            TimeSpan.FromMinutes(_cacheSettings.ProductQuestionsCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }
}

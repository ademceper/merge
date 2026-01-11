using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Queries.GetUserQuestions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetUserQuestionsQueryHandler : IRequestHandler<GetUserQuestionsQuery, PagedResult<ProductQuestionDto>>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetUserQuestionsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_USER_QUESTIONS = "user_questions_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // User questions can change frequently

    public GetUserQuestionsQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetUserQuestionsQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<ProductQuestionDto>> Handle(GetUserQuestionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user questions. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize
            ? _paginationSettings.MaxPageSize
            : request.PageSize;

        var cacheKey = $"{CACHE_KEY_USER_QUESTIONS}{request.UserId}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for user questions. Fetching from database.");

                var query = _context.Set<ProductQuestion>()
                    .AsNoTracking()
                    .Include(q => q.Product)
                    .Include(q => q.User)
                    .Include(q => q.Answers.Where(a => a.IsApproved))
                        .ThenInclude(a => a.User)
                    .Where(q => q.UserId == request.UserId);

                var totalCount = await query.CountAsync(cancellationToken);

                var questions = await query
                    .OrderByDescending(q => q.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var questionIds = questions.Select(q => q.Id).ToList();
                var userVotes = questionIds.Any()
                    ? await _context.Set<QuestionHelpfulness>()
                        .AsNoTracking()
                        .Where(qh => questionIds.Contains(qh.QuestionId) && qh.UserId == request.UserId)
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
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedResult!;
    }
}

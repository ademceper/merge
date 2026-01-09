using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Queries.GetPageBuilderBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPageBuilderBySlugQueryHandler : IRequestHandler<GetPageBuilderBySlugQuery, PageBuilderDto?>
{
    private readonly IDbContext _context;
    private readonly IRepository<PageBuilder> _pageBuilderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPageBuilderBySlugQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PAGE_BY_SLUG = "page_builder_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetPageBuilderBySlugQueryHandler(
        IDbContext context,
        IRepository<PageBuilder> pageBuilderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetPageBuilderBySlugQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _pageBuilderRepository = pageBuilderRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<PageBuilderDto?> Handle(GetPageBuilderBySlugQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving page builder with Slug: {Slug}, TrackView: {TrackView}", request.Slug, request.TrackView);

        var cacheKey = $"{CACHE_KEY_PAGE_BY_SLUG}{request.Slug}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedPage = await _cache.GetAsync<PageBuilderDto>(cacheKey, cancellationToken);
        if (cachedPage != null && !request.TrackView)
        {
            _logger.LogInformation("Cache hit for page builder. Slug: {Slug}", request.Slug);
            return cachedPage;
        }

        _logger.LogInformation("Cache miss for page builder. Slug: {Slug}", request.Slug);

        var pageBuilder = request.TrackView
            ? await _context.Set<PageBuilder>()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Slug == request.Slug && pb.Status == ContentStatus.Published, cancellationToken)
            : await _context.Set<PageBuilder>()
                .AsNoTracking()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Slug == request.Slug && pb.Status == ContentStatus.Published, cancellationToken);

        if (pageBuilder == null)
        {
            _logger.LogWarning("Page builder not found with Slug: {Slug}", request.Slug);
            return null;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.TrackView)
        {
            pageBuilder.IncrementViewCount();
            await _pageBuilderRepository.UpdateAsync(pageBuilder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Successfully retrieved page builder {Slug}", request.Slug);

        var pageDto = _mapper.Map<PageBuilderDto>(pageBuilder);

        if (!request.TrackView)
        {
            await _cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);
        }

        return pageDto;
    }
}


using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Queries.GetLandingPageBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetLandingPageBySlugQueryHandler : IRequestHandler<GetLandingPageBySlugQuery, LandingPageDto?>
{
    private readonly IDbContext _context;
    private readonly IRepository<LandingPage> _landingPageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLandingPageBySlugQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PAGE_BY_SLUG = "landing_page_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetLandingPageBySlugQueryHandler(
        IDbContext context,
        IRepository<LandingPage> landingPageRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetLandingPageBySlugQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _landingPageRepository = landingPageRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<LandingPageDto?> Handle(GetLandingPageBySlugQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving landing page with Slug: {Slug}, TrackView: {TrackView}", request.Slug, request.TrackView);

        var cacheKey = $"{CACHE_KEY_PAGE_BY_SLUG}{request.Slug}";

        // ✅ BOLUM 10.2: Redis distributed cache for single landing page
        var cachedPage = await _cache.GetAsync<LandingPageDto>(cacheKey, cancellationToken);
        if (cachedPage != null && !request.TrackView) // Don't use cache if tracking view (need to update)
        {
            _logger.LogInformation("Cache hit for landing page. Slug: {Slug}", request.Slug);
            return cachedPage;
        }

        _logger.LogInformation("Cache miss for landing page. Slug: {Slug}", request.Slug);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries (unless tracking view)
        var landingPage = request.TrackView
            ? await _context.Set<LandingPage>()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Slug == request.Slug && lp.Status == ContentStatus.Published && lp.IsActive, cancellationToken)
            : await _context.Set<LandingPage>()
                .AsNoTracking()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Slug == request.Slug && lp.Status == ContentStatus.Published && lp.IsActive, cancellationToken);

        if (landingPage == null)
        {
            _logger.LogWarning("Landing page not found with Slug: {Slug}", request.Slug);
            return null;
        }

        // A/B testing variant selection
        if (landingPage.EnableABTesting && landingPage.Variants != null && landingPage.Variants.Any())
        {
            var variants = landingPage.Variants.Where(v => v.IsActive).ToList();
            if (variants.Any())
            {
                // Simple random selection based on traffic split (can be improved with proper A/B testing logic)
                // ✅ THREAD SAFETY: Random.Shared kullan (new Random() thread-safe değil)
                var random = Random.Shared;
                var selectedVariant = variants.OrderBy(v => random.Next()).FirstOrDefault();
                if (selectedVariant != null)
                {
                    landingPage = selectedVariant;
                }
            }
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.TrackView)
        {
            landingPage.IncrementViewCount();
            await _landingPageRepository.UpdateAsync(landingPage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Successfully retrieved landing page {Slug}", request.Slug);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var pageDto = _mapper.Map<LandingPageDto>(landingPage);

        // Cache the result (only if not tracking view)
        if (!request.TrackView)
        {
            await _cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);
        }

        return pageDto;
    }
}


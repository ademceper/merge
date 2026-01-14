using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetLandingPageById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetLandingPageByIdQueryHandler : IRequestHandler<GetLandingPageByIdQuery, LandingPageDto?>
{
    private readonly IDbContext _context;
    private readonly Merge.Application.Interfaces.IRepository<LandingPage> _landingPageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLandingPageByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PAGE_BY_ID = "landing_page_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetLandingPageByIdQueryHandler(
        IDbContext context,
        Merge.Application.Interfaces.IRepository<LandingPage> landingPageRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetLandingPageByIdQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _landingPageRepository = landingPageRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<LandingPageDto?> Handle(GetLandingPageByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving landing page with Id: {LandingPageId}, TrackView: {TrackView}", request.Id, request.TrackView);

        var cacheKey = $"{CACHE_KEY_PAGE_BY_ID}{request.Id}";

        // ✅ BOLUM 10.2: Redis distributed cache for single landing page
        var cachedPage = await _cache.GetAsync<LandingPageDto>(cacheKey, cancellationToken);
        if (cachedPage != null && !request.TrackView) // Don't use cache if tracking view (need to update)
        {
            _logger.LogInformation("Cache hit for landing page. LandingPageId: {LandingPageId}", request.Id);
            return cachedPage;
        }

        _logger.LogInformation("Cache miss for landing page. LandingPageId: {LandingPageId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries (unless tracking view)
        var landingPage = request.TrackView
            ? await _context.Set<LandingPage>()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Id == request.Id, cancellationToken)
            : await _context.Set<LandingPage>()
                .AsNoTracking()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Id == request.Id, cancellationToken);

        if (landingPage == null)
        {
            _logger.LogWarning("Landing page not found with Id: {LandingPageId}", request.Id);
            return null;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.TrackView && landingPage.Status == ContentStatus.Published && landingPage.IsActive)
        {
            landingPage.IncrementViewCount();
            await _landingPageRepository.UpdateAsync(landingPage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Successfully retrieved landing page {LandingPageId}", request.Id);

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


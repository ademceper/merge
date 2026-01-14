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

namespace Merge.Application.Content.Queries.GetPageBuilderById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPageBuilderByIdQueryHandler : IRequestHandler<GetPageBuilderByIdQuery, PageBuilderDto?>
{
    private readonly IDbContext _context;
    private readonly Merge.Application.Interfaces.IRepository<PageBuilder> _pageBuilderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPageBuilderByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PAGE_BY_ID = "page_builder_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetPageBuilderByIdQueryHandler(
        IDbContext context,
        Merge.Application.Interfaces.IRepository<PageBuilder> pageBuilderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetPageBuilderByIdQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _pageBuilderRepository = pageBuilderRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<PageBuilderDto?> Handle(GetPageBuilderByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving page builder with Id: {PageBuilderId}, TrackView: {TrackView}", request.Id, request.TrackView);

        var cacheKey = $"{CACHE_KEY_PAGE_BY_ID}{request.Id}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedPage = await _cache.GetAsync<PageBuilderDto>(cacheKey, cancellationToken);
        if (cachedPage != null && !request.TrackView)
        {
            _logger.LogInformation("Cache hit for page builder. PageBuilderId: {PageBuilderId}", request.Id);
            return cachedPage;
        }

        _logger.LogInformation("Cache miss for page builder. PageBuilderId: {PageBuilderId}", request.Id);

        var pageBuilder = request.TrackView
            ? await _context.Set<PageBuilder>()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Id == request.Id, cancellationToken)
            : await _context.Set<PageBuilder>()
                .AsNoTracking()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Id == request.Id, cancellationToken);

        if (pageBuilder == null)
        {
            _logger.LogWarning("Page builder not found with Id: {PageBuilderId}", request.Id);
            return null;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.TrackView && pageBuilder.Status == ContentStatus.Published && pageBuilder.IsActive)
        {
            pageBuilder.IncrementViewCount();
            await _pageBuilderRepository.UpdateAsync(pageBuilder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Successfully retrieved page builder {PageBuilderId}", request.Id);

        var pageDto = _mapper.Map<PageBuilderDto>(pageBuilder);

        if (!request.TrackView)
        {
            await _cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);
        }

        return pageDto;
    }
}


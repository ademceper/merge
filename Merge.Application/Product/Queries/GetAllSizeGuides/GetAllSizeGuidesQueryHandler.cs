using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Queries.GetAllSizeGuides;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAllSizeGuidesQueryHandler : IRequestHandler<GetAllSizeGuidesQuery, IEnumerable<SizeGuideDto>>
{
    private readonly IDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<GetAllSizeGuidesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ALL_SIZE_GUIDES = "size_guides_all";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(30); // Size guides change less frequently

    public GetAllSizeGuidesQueryHandler(
        IDbContext context,
        AutoMapper.IMapper mapper,
        ILogger<GetAllSizeGuidesQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<SizeGuideDto>> Handle(GetAllSizeGuidesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all size guides");

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cachedSizeGuides = await _cache.GetAsync<IEnumerable<SizeGuideDto>>(CACHE_KEY_ALL_SIZE_GUIDES, cancellationToken);
        if (cachedSizeGuides != null)
        {
            _logger.LogInformation("Size guides retrieved from cache");
            return cachedSizeGuides;
        }

        _logger.LogInformation("Cache miss for all size guides. Fetching from database.");

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var sizeGuides = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .Where(sg => sg.IsActive)
            .ToListAsync(cancellationToken);

        var sizeGuideDtos = _mapper.Map<IEnumerable<SizeGuideDto>>(sizeGuides).ToList();

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        await _cache.SetAsync(CACHE_KEY_ALL_SIZE_GUIDES, sizeGuideDtos, CACHE_EXPIRATION, cancellationToken);

        _logger.LogInformation("Retrieved all size guides. Count: {Count}", sizeGuides.Count);

        return sizeGuideDtos;
    }
}

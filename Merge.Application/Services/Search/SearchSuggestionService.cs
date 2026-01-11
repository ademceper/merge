using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Search;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.DTOs.Search;


namespace Merge.Application.Services.Search;

public class SearchSuggestionService : ISearchSuggestionService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchSuggestionService> _logger;

    public SearchSuggestionService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<SearchSuggestionService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<AutocompleteResultDto> GetAutocompleteSuggestionsAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return new AutocompleteResultDto(
                Products: new List<ProductSuggestionDto>(),
                Categories: new List<string>(),
                Brands: new List<string>(),
                PopularSearches: new List<string>()
            );
        }

        var normalizedQuery = query.ToLower().Trim();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Product suggestions
        var productSuggestions = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive &&
                       (p.Name.ToLower().Contains(normalizedQuery) ||
                        p.Description.ToLower().Contains(normalizedQuery)))
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
        
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var productSuggestionDtos = _mapper.Map<IEnumerable<ProductSuggestionDto>>(productSuggestions).ToList();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // Category suggestions
        var categorySuggestions = await _context.Set<Category>()
            .AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(normalizedQuery))
            .OrderBy(c => c.Name)
            .Take(5)
            .Select(c => c.Name)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Brand suggestions
        var brandSuggestions = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       p.Brand.ToLower().Contains(normalizedQuery))
            .Select(p => p.Brand)
            .Distinct()
            .Take(5)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ps.IsDeleted (Global Query Filter)
        // Popular searches containing the query
        var popularSearches = await _context.Set<PopularSearch>()
            .AsNoTracking()
            .Where(ps => ps.SearchTerm.ToLower().Contains(normalizedQuery))
            .OrderByDescending(ps => ps.SearchCount)
            .Take(5)
            .Select(ps => ps.SearchTerm)
            .ToListAsync(cancellationToken);

        return new AutocompleteResultDto(
            Products: productSuggestionDtos,
            Categories: categorySuggestions,
            Brands: brandSuggestions,
            PopularSearches: popularSearches
        );
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<string>> GetPopularSearchesAsync(int maxResults = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ps.IsDeleted (Global Query Filter)
        var popularSearches = await _context.Set<PopularSearch>()
            .AsNoTracking()
            .OrderByDescending(ps => ps.SearchCount)
            .Take(maxResults)
            .Select(ps => ps.SearchTerm)
            .ToListAsync(cancellationToken);

        return popularSearches;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task RecordSearchAsync(string searchTerm, Guid? userId, int resultCount, string? userAgent = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return;
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Search kaydediliyor. SearchTerm: {SearchTerm}, UserId: {UserId}, ResultCount: {ResultCount}",
            searchTerm, userId, resultCount);

        var normalizedTerm = searchTerm.Trim();

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // Record in search history
        var searchHistory = SearchHistory.Create(
            userId: userId,
            searchTerm: normalizedTerm,
            resultCount: resultCount,
            userAgent: userAgent,
            ipAddress: ipAddress);

        await _context.Set<SearchHistory>().AddAsync(searchHistory, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !ps.IsDeleted (Global Query Filter)
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method ve Domain Method kullanımı
        // Update or create popular search
        var popularSearch = await _context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => ps.SearchTerm.ToLower() == normalizedTerm.ToLower(), cancellationToken);

        if (popularSearch == null)
        {
            popularSearch = PopularSearch.Create(normalizedTerm);
            await _context.Set<PopularSearch>().AddAsync(popularSearch, cancellationToken);
        }
        else
        {
            popularSearch.IncrementSearchCount();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Search kaydedildi. SearchTerm: {SearchTerm}, UserId: {UserId}",
            searchTerm, userId);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task RecordClickAsync(Guid searchHistoryId, Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sh.IsDeleted (Global Query Filter)
        var searchHistory = await _context.Set<SearchHistory>()
            .FirstOrDefaultAsync(sh => sh.Id == searchHistoryId, cancellationToken);

        if (searchHistory == null)
        {
            return;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        searchHistory.RecordClick(productId);

        // ✅ PERFORMANCE: Removed manual !ps.IsDeleted (Global Query Filter)
        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // Update popular search click-through rate
        var popularSearch = await _context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => ps.SearchTerm.ToLower() == searchHistory.SearchTerm.ToLower(), cancellationToken);

        if (popularSearch != null)
        {
            popularSearch.IncrementClickThroughCount();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Search click kaydedildi. SearchHistoryId: {SearchHistoryId}, ProductId: {ProductId}",
            searchHistoryId, productId);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<SearchSuggestionDto>> GetTrendingSearchesAsync(int days = 7, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sh.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var trendingSearches = await _context.Set<SearchHistory>()
            .AsNoTracking()
            .Where(sh => sh.CreatedAt >= startDate)
            .GroupBy(sh => sh.SearchTerm.ToLower())
            .Select(g => new SearchSuggestionDto(
                g.First().SearchTerm,
                "Trending",
                g.Count(),
                (Guid?)null
            ))
            .OrderByDescending(s => s.Frequency)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        return trendingSearches;
    }
}

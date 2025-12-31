using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Search;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Search;


namespace Merge.Application.Services.Search;

public class SearchSuggestionService : ISearchSuggestionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchSuggestionService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AutocompleteResultDto> GetAutocompleteSuggestionsAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return new AutocompleteResultDto();
        }

        var normalizedQuery = query.ToLower().Trim();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Product suggestions
        var productSuggestions = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive &&
                       (p.Name.ToLower().Contains(normalizedQuery) ||
                        p.Description.ToLower().Contains(normalizedQuery)))
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync();
        
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var productSuggestionDtos = _mapper.Map<IEnumerable<ProductSuggestionDto>>(productSuggestions).ToList();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // Category suggestions
        var categorySuggestions = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(normalizedQuery))
            .OrderBy(c => c.Name)
            .Take(5)
            .Select(c => c.Name)
            .ToListAsync();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // Brand suggestions
        var brandSuggestions = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       p.Brand.ToLower().Contains(normalizedQuery))
            .Select(p => p.Brand)
            .Distinct()
            .Take(5)
            .ToListAsync();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ps.IsDeleted (Global Query Filter)
        // Popular searches containing the query
        var popularSearches = await _context.Set<PopularSearch>()
            .AsNoTracking()
            .Where(ps => ps.SearchTerm.ToLower().Contains(normalizedQuery))
            .OrderByDescending(ps => ps.SearchCount)
            .Take(5)
            .Select(ps => ps.SearchTerm)
            .ToListAsync();

        return new AutocompleteResultDto
        {
            Products = productSuggestionDtos,
            Categories = categorySuggestions,
            Brands = brandSuggestions,
            PopularSearches = popularSearches
        };
    }

    public async Task<IEnumerable<string>> GetPopularSearchesAsync(int maxResults = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ps.IsDeleted (Global Query Filter)
        var popularSearches = await _context.Set<PopularSearch>()
            .AsNoTracking()
            .OrderByDescending(ps => ps.SearchCount)
            .Take(maxResults)
            .Select(ps => ps.SearchTerm)
            .ToListAsync();

        return popularSearches;
    }

    public async Task RecordSearchAsync(string searchTerm, Guid? userId, int resultCount, string? userAgent = null, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return;
        }

        var normalizedTerm = searchTerm.Trim();

        // Record in search history
        var searchHistory = new SearchHistory
        {
            UserId = userId,
            SearchTerm = normalizedTerm,
            ResultCount = resultCount,
            UserAgent = userAgent,
            IpAddress = ipAddress
        };

        await _context.Set<SearchHistory>().AddAsync(searchHistory);

        // ✅ PERFORMANCE: Removed manual !ps.IsDeleted (Global Query Filter)
        // Update or create popular search
        var popularSearch = await _context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => ps.SearchTerm.ToLower() == normalizedTerm.ToLower());

        if (popularSearch == null)
        {
            popularSearch = new PopularSearch
            {
                SearchTerm = normalizedTerm,
                SearchCount = 1,
                ClickThroughCount = 0,
                ClickThroughRate = 0,
                LastSearchedAt = DateTime.UtcNow
            };
            await _context.Set<PopularSearch>().AddAsync(popularSearch);
        }
        else
        {
            popularSearch.SearchCount++;
            popularSearch.LastSearchedAt = DateTime.UtcNow;
            popularSearch.ClickThroughRate = popularSearch.SearchCount > 0
                ? (decimal)popularSearch.ClickThroughCount / popularSearch.SearchCount * 100
                : 0;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RecordClickAsync(Guid searchHistoryId, Guid productId)
    {
        // ✅ PERFORMANCE: Removed manual !sh.IsDeleted (Global Query Filter)
        var searchHistory = await _context.Set<SearchHistory>()
            .FirstOrDefaultAsync(sh => sh.Id == searchHistoryId);

        if (searchHistory == null)
        {
            return;
        }

        searchHistory.ClickedResult = true;
        searchHistory.ClickedProductId = productId;

        // ✅ PERFORMANCE: Removed manual !ps.IsDeleted (Global Query Filter)
        // Update popular search click-through rate
        var popularSearch = await _context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => ps.SearchTerm.ToLower() == searchHistory.SearchTerm.ToLower());

        if (popularSearch != null)
        {
            popularSearch.ClickThroughCount++;
            popularSearch.ClickThroughRate = (decimal)popularSearch.ClickThroughCount / popularSearch.SearchCount * 100;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<SearchSuggestionDto>> GetTrendingSearchesAsync(int days = 7, int maxResults = 10)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sh.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        var trendingSearches = await _context.Set<SearchHistory>()
            .AsNoTracking()
            .Where(sh => sh.CreatedAt >= startDate)
            .GroupBy(sh => sh.SearchTerm.ToLower())
            .Select(g => new SearchSuggestionDto
            {
                Term = g.First().SearchTerm,
                Type = "Trending",
                Frequency = g.Count()
            })
            .OrderByDescending(s => s.Frequency)
            .Take(maxResults)
            .ToListAsync();

        return trendingSearches;
    }
}

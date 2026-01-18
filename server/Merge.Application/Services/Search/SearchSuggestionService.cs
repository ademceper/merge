using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Search;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.DTOs.Search;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Search;

public class SearchSuggestionService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<SearchSuggestionService> logger) : ISearchSuggestionService
{

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

        var trimmedQuery = query.Trim();

        // Product suggestions
        var productSuggestions = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive &&
                       (EF.Functions.ILike(p.Name, $"%{trimmedQuery}%") ||
                        EF.Functions.ILike(p.Description, $"%{trimmedQuery}%")))
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
        
        var productSuggestionDtos = mapper.Map<IEnumerable<ProductSuggestionDto>>(productSuggestions).ToList();

        // Category suggestions
        var categorySuggestions = await context.Set<Category>()
            .AsNoTracking()
            .Where(c => EF.Functions.ILike(c.Name, $"%{trimmedQuery}%"))
            .OrderBy(c => c.Name)
            .Take(5)
            .Select(c => c.Name)
            .ToListAsync(cancellationToken);

        // Brand suggestions
        var brandSuggestions = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive && p.Brand != null &&
                       EF.Functions.ILike(p.Brand, $"%{trimmedQuery}%"))
            .Select(p => p.Brand)
            .Distinct()
            .Take(5)
            .ToListAsync(cancellationToken);

        // Popular searches containing the query
        var popularSearches = await context.Set<PopularSearch>()
            .AsNoTracking()
            .Where(ps => EF.Functions.ILike(ps.SearchTerm, $"%{trimmedQuery}%"))
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

    public async Task<IEnumerable<string>> GetPopularSearchesAsync(int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var popularSearches = await context.Set<PopularSearch>()
            .AsNoTracking()
            .OrderByDescending(ps => ps.SearchCount)
            .Take(maxResults)
            .Select(ps => ps.SearchTerm)
            .ToListAsync(cancellationToken);

        return popularSearches;
    }

    public async Task RecordSearchAsync(string searchTerm, Guid? userId, int resultCount, string? userAgent = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return;
        }

        logger.LogInformation(
            "Search kaydediliyor. SearchTerm: {SearchTerm}, UserId: {UserId}, ResultCount: {ResultCount}",
            searchTerm, userId, resultCount);

        var normalizedTerm = searchTerm.Trim();

        // Record in search history
        var searchHistory = SearchHistory.Create(
            userId: userId,
            searchTerm: normalizedTerm,
            resultCount: resultCount,
            userAgent: userAgent,
            ipAddress: ipAddress);

        await context.Set<SearchHistory>().AddAsync(searchHistory, cancellationToken);

        // Update or create popular search
        var popularSearch = await context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => EF.Functions.ILike(ps.SearchTerm, normalizedTerm), cancellationToken);

        if (popularSearch is null)
        {
            popularSearch = PopularSearch.Create(normalizedTerm);
            await context.Set<PopularSearch>().AddAsync(popularSearch, cancellationToken);
        }
        else
        {
            popularSearch.IncrementSearchCount();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Search kaydedildi. SearchTerm: {SearchTerm}, UserId: {UserId}",
            searchTerm, userId);
    }

    public async Task RecordClickAsync(Guid searchHistoryId, Guid productId, CancellationToken cancellationToken = default)
    {
        var searchHistory = await context.Set<SearchHistory>()
            .FirstOrDefaultAsync(sh => sh.Id == searchHistoryId, cancellationToken);

        if (searchHistory is null)
        {
            return;
        }

        searchHistory.RecordClick(productId);

        // Update popular search click-through rate
        var popularSearch = await context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => EF.Functions.ILike(ps.SearchTerm, searchHistory.SearchTerm), cancellationToken);

        if (popularSearch is not null)
        {
            popularSearch.IncrementClickThroughCount();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Search click kaydedildi. SearchHistoryId: {SearchHistoryId}, ProductId: {ProductId}",
            searchHistoryId, productId);
    }

    public async Task<IEnumerable<SearchSuggestionDto>> GetTrendingSearchesAsync(int days = 7, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var trendingSearches = await context.Set<SearchHistory>()
            .AsNoTracking()
            .Where(sh => sh.CreatedAt >= startDate)
            .GroupBy(sh => sh.SearchTerm.ToLower())
            .Where(g => g.Any()) // ✅ ERROR HANDLING FIX: Ensure group has elements before First()
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

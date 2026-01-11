using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Search;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Domain.Entities;

namespace Merge.Application.Search.Queries.GetAutocompleteSuggestions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAutocompleteSuggestionsQueryHandler : IRequestHandler<GetAutocompleteSuggestionsQuery, AutocompleteResultDto>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAutocompleteSuggestionsQueryHandler> _logger;
    private readonly SearchSettings _searchSettings;

    public GetAutocompleteSuggestionsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAutocompleteSuggestionsQueryHandler> logger,
        IOptions<SearchSettings> searchSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _searchSettings = searchSettings.Value;
    }

    public async Task<AutocompleteResultDto> Handle(GetAutocompleteSuggestionsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Autocomplete suggestions isteniyor. Query: {Query}, MaxResults: {MaxResults}",
            request.Query, request.MaxResults);

        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < _searchSettings.MinAutocompleteQueryLength)
        {
            _logger.LogDebug(
                "Autocomplete query çok kısa veya boş. Query: {Query}, MinLength: {MinLength}",
                request.Query, _searchSettings.MinAutocompleteQueryLength);
            return new AutocompleteResultDto(
                Products: Array.Empty<ProductSuggestionDto>(),
                Categories: Array.Empty<string>(),
                Brands: Array.Empty<string>(),
                PopularSearches: Array.Empty<string>()
            );
        }

        var normalizedQuery = request.Query.ToLower().Trim();
        var maxResults = request.MaxResults > _searchSettings.MaxAutocompleteResults
            ? _searchSettings.MaxAutocompleteResults
            : request.MaxResults;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var productSuggestions = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive &&
                       (EF.Functions.ILike(p.Name, $"%{normalizedQuery}%") ||
                        EF.Functions.ILike(p.Description, $"%{normalizedQuery}%")))
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var productSuggestionDtos = _mapper.Map<IEnumerable<ProductSuggestionDto>>(productSuggestions).ToList();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var categorySuggestions = await _context.Set<Category>()
            .AsNoTracking()
            .Where(c => EF.Functions.ILike(c.Name, $"%{normalizedQuery}%"))
            .OrderBy(c => c.Name)
            .Take(5)
            .Select(c => c.Name)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var brandSuggestions = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       !string.IsNullOrEmpty(p.Brand) &&
                       EF.Functions.ILike(p.Brand, $"%{normalizedQuery}%"))
            .Select(p => p.Brand)
            .Distinct()
            .Take(5)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ps.IsDeleted (Global Query Filter)
        var popularSearches = await _context.Set<PopularSearch>()
            .AsNoTracking()
            .Where(ps => EF.Functions.ILike(ps.SearchTerm, $"%{normalizedQuery}%"))
            .OrderByDescending(ps => ps.SearchCount)
            .Take(5)
            .Select(ps => ps.SearchTerm)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Autocomplete suggestions tamamlandı. Query: {Query}, ProductCount: {ProductCount}, CategoryCount: {CategoryCount}, BrandCount: {BrandCount}, PopularSearchCount: {PopularSearchCount}",
            request.Query, productSuggestionDtos.Count, categorySuggestions.Count, brandSuggestions.Count, popularSearches.Count);

        return new AutocompleteResultDto(
            Products: productSuggestionDtos,
            Categories: categorySuggestions,
            Brands: brandSuggestions,
            PopularSearches: popularSearches
        );
    }
}

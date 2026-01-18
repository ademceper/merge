using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Search;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Queries.GetAutocompleteSuggestions;

public class GetAutocompleteSuggestionsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAutocompleteSuggestionsQueryHandler> logger, IOptions<SearchSettings> searchSettings) : IRequestHandler<GetAutocompleteSuggestionsQuery, AutocompleteResultDto>
{
    private readonly SearchSettings searchConfig = searchSettings.Value;

    public async Task<AutocompleteResultDto> Handle(GetAutocompleteSuggestionsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Autocomplete suggestions isteniyor. Query: {Query}, MaxResults: {MaxResults}",
            request.Query, request.MaxResults);

        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < searchConfig.MinAutocompleteQueryLength)
        {
            logger.LogDebug(
                "Autocomplete query çok kısa veya boş. Query: {Query}, MinLength: {MinLength}",
                request.Query, searchConfig.MinAutocompleteQueryLength);
            return new AutocompleteResultDto(
                Products: Array.Empty<ProductSuggestionDto>(),
                Categories: Array.Empty<string>(),
                Brands: Array.Empty<string>(),
                PopularSearches: Array.Empty<string>()
            );
        }

        var normalizedQuery = request.Query.ToLower().Trim();
        var maxResults = request.MaxResults > searchConfig.MaxAutocompleteResults
            ? searchConfig.MaxAutocompleteResults
            : request.MaxResults;

        var productSuggestions = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive &&
                       (EF.Functions.ILike(p.Name, $"%{normalizedQuery}%") ||
                        EF.Functions.ILike(p.Description, $"%{normalizedQuery}%")))
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewCount)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        var productSuggestionDtos = mapper.Map<IEnumerable<ProductSuggestionDto>>(productSuggestions).ToList();

        var categorySuggestions = await context.Set<Category>()
            .AsNoTracking()
            .Where(c => EF.Functions.ILike(c.Name, $"%{normalizedQuery}%"))
            .OrderBy(c => c.Name)
            .Take(5)
            .Select(c => c.Name)
            .ToListAsync(cancellationToken);

        var brandSuggestions = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive &&
                       !string.IsNullOrEmpty(p.Brand) &&
                       EF.Functions.ILike(p.Brand, $"%{normalizedQuery}%"))
            .Select(p => p.Brand)
            .Distinct()
            .Take(5)
            .ToListAsync(cancellationToken);

        var popularSearches = await context.Set<PopularSearch>()
            .AsNoTracking()
            .Where(ps => EF.Functions.ILike(ps.SearchTerm, $"%{normalizedQuery}%"))
            .OrderByDescending(ps => ps.SearchCount)
            .Take(5)
            .Select(ps => ps.SearchTerm)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
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

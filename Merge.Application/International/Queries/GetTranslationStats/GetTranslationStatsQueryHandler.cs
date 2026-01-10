using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.International.Queries.GetTranslationStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetTranslationStatsQueryHandler : IRequestHandler<GetTranslationStatsQuery, TranslationStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetTranslationStatsQueryHandler> _logger;

    public GetTranslationStatsQueryHandler(
        IDbContext context,
        ILogger<GetTranslationStatsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TranslationStatsDto> Handle(GetTranslationStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting translation stats");

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var totalLanguages = await _context.Set<Language>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var activeLanguages = await _context.Set<Language>()
            .AsNoTracking()
            .CountAsync(l => l.IsActive, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var defaultLanguage = await _context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsDefault)
            .Select(l => l.Code)
            .FirstOrDefaultAsync(cancellationToken) ?? "en";

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var languageCoverage = await _context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Select(l => new LanguageCoverageDto(
                l.Code,
                l.Name,
                _context.Set<ProductTranslation>()
                    .AsNoTracking()
                    .Count(pt => pt.LanguageCode == l.Code),
                totalProducts,
                totalProducts > 0
                    ? (decimal)_context.Set<ProductTranslation>()
                        .AsNoTracking()
                        .Count(pt => pt.LanguageCode == l.Code) / totalProducts * 100
                    : 0))
            .ToListAsync(cancellationToken);

        return new TranslationStatsDto(
            TotalLanguages: totalLanguages,
            ActiveLanguages: activeLanguages,
            DefaultLanguage: defaultLanguage,
            LanguageCoverage: languageCoverage.AsReadOnly());
    }
}


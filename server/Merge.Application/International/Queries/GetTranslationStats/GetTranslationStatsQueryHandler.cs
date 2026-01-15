using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetTranslationStats;

public class GetTranslationStatsQueryHandler(
    IDbContext context,
    ILogger<GetTranslationStatsQueryHandler> logger) : IRequestHandler<GetTranslationStatsQuery, TranslationStatsDto>
{
    public async Task<TranslationStatsDto> Handle(GetTranslationStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting translation stats");

        var totalLanguages = await context.Set<Language>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeLanguages = await context.Set<Language>()
            .AsNoTracking()
            .CountAsync(l => l.IsActive, cancellationToken);

        var defaultLanguage = await context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsDefault)
            .Select(l => l.Code)
            .FirstOrDefaultAsync(cancellationToken) ?? "en";

        var totalProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var languageCoverage = await context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Select(l => new LanguageCoverageDto(
                l.Code,
                l.Name,
                context.Set<ProductTranslation>()
                    .AsNoTracking()
                    .Count(pt => pt.LanguageCode == l.Code),
                totalProducts,
                totalProducts > 0
                    ? (decimal)context.Set<ProductTranslation>()
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

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetStaticTranslations;

public class GetStaticTranslationsQueryHandler(
    IDbContext context,
    ILogger<GetStaticTranslationsQueryHandler> logger) : IRequestHandler<GetStaticTranslationsQuery, Dictionary<string, string>>
{
    public async Task<Dictionary<string, string>> Handle(GetStaticTranslationsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting static translations. LanguageCode: {LanguageCode}, Category: {Category}", 
            request.LanguageCode, request.Category);

        var query = context.Set<StaticTranslation>()
            .AsNoTracking()
            .Where(st => st.LanguageCode.ToLower() == request.LanguageCode.ToLower());

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(st => st.Category == request.Category);
        }

        var translations = await query
            .Take(1000)
            .ToDictionaryAsync(st => st.Key, st => st.Value, cancellationToken);

        return translations;
    }
}

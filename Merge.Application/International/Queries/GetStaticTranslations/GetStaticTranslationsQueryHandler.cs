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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetStaticTranslationsQueryHandler : IRequestHandler<GetStaticTranslationsQuery, Dictionary<string, string>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetStaticTranslationsQueryHandler> _logger;

    public GetStaticTranslationsQueryHandler(
        IDbContext context,
        ILogger<GetStaticTranslationsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Dictionary<string, string>> Handle(GetStaticTranslationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting static translations. LanguageCode: {LanguageCode}, Category: {Category}", 
            request.LanguageCode, request.Category);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !st.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var query = _context.Set<StaticTranslation>()
            .AsNoTracking()
            .Where(st => st.LanguageCode.ToLower() == request.LanguageCode.ToLower());

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(st => st.Category == request.Category);
        }

        // ✅ Güvenlik: Maksimum 1000 çeviri
        var translations = await query
            .Take(1000)
            .ToDictionaryAsync(st => st.Key, st => st.Value, cancellationToken);

        return translations;
    }
}


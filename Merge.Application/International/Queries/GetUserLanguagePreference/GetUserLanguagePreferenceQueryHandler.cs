using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.International.Queries.GetUserLanguagePreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetUserLanguagePreferenceQueryHandler : IRequestHandler<GetUserLanguagePreferenceQuery, string>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetUserLanguagePreferenceQueryHandler> _logger;

    public GetUserLanguagePreferenceQueryHandler(
        IDbContext context,
        ILogger<GetUserLanguagePreferenceQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> Handle(GetUserLanguagePreferenceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user language preference. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserLanguagePreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (preference != null)
        {
            return preference.LanguageCode;
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var defaultLanguage = await _context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);

        return defaultLanguage?.Code ?? "en";
    }
}


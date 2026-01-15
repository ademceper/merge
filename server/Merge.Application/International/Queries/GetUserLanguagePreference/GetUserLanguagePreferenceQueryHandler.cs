using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetUserLanguagePreference;

public class GetUserLanguagePreferenceQueryHandler(
    IDbContext context,
    ILogger<GetUserLanguagePreferenceQueryHandler> logger) : IRequestHandler<GetUserLanguagePreferenceQuery, string>
{
    public async Task<string> Handle(GetUserLanguagePreferenceQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting user language preference. UserId: {UserId}", request.UserId);

        var preference = await context.Set<UserLanguagePreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (preference != null)
        {
            return preference.LanguageCode;
        }

        var defaultLanguage = await context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);

        return defaultLanguage?.Code ?? "en";
    }
}

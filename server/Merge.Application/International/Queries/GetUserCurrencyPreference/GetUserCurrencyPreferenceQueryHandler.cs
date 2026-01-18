using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetUserCurrencyPreference;

public class GetUserCurrencyPreferenceQueryHandler(
    IDbContext context,
    ILogger<GetUserCurrencyPreferenceQueryHandler> logger) : IRequestHandler<GetUserCurrencyPreferenceQuery, string>
{
    public async Task<string> Handle(GetUserCurrencyPreferenceQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting user currency preference. UserId: {UserId}", request.UserId);

        var preference = await context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (preference is not null)
        {
            return preference.CurrencyCode;
        }

        // Return base currency if no preference set
        var baseCurrency = await context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IsBaseCurrency, cancellationToken);

        return baseCurrency?.Code ?? "USD";
    }
}

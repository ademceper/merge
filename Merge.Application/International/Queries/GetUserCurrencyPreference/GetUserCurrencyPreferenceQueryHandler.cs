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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetUserCurrencyPreferenceQueryHandler : IRequestHandler<GetUserCurrencyPreferenceQuery, string>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetUserCurrencyPreferenceQueryHandler> _logger;

    public GetUserCurrencyPreferenceQueryHandler(
        IDbContext context,
        ILogger<GetUserCurrencyPreferenceQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> Handle(GetUserCurrencyPreferenceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user currency preference. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (preference != null)
        {
            return preference.CurrencyCode;
        }

        // Return base currency if no preference set
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var baseCurrency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IsBaseCurrency, cancellationToken);

        return baseCurrency?.Code ?? "USD";
    }
}


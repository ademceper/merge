using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.FormatPrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class FormatPriceQueryHandler : IRequestHandler<FormatPriceQuery, string>
{
    private readonly IDbContext _context;
    private readonly ILogger<FormatPriceQueryHandler> _logger;

    public FormatPriceQueryHandler(
        IDbContext context,
        ILogger<FormatPriceQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> Handle(FormatPriceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Formatting price. Amount: {Amount}, CurrencyCode: {CurrencyCode}", 
            request.Amount, request.CurrencyCode);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.CurrencyCode.ToUpper(), cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency not found. CurrencyCode: {CurrencyCode}", request.CurrencyCode);
            return request.Amount.ToString("N2");
        }

        var roundedAmount = Math.Round(request.Amount, currency.DecimalPlaces);
        var formattedAmount = roundedAmount.ToString($"N{currency.DecimalPlaces}");

        return currency.Format
            .Replace("{symbol}", currency.Symbol)
            .Replace("{amount}", formattedAmount)
            .Replace("{code}", currency.Code);
    }
}


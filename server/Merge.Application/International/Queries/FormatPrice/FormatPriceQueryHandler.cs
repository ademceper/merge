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

public class FormatPriceQueryHandler(
    IDbContext context,
    ILogger<FormatPriceQueryHandler> logger) : IRequestHandler<FormatPriceQuery, string>
{
    public async Task<string> Handle(FormatPriceQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Formatting price. Amount: {Amount}, CurrencyCode: {CurrencyCode}", 
            request.Amount, request.CurrencyCode);

        var currency = await context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Code, request.CurrencyCode), cancellationToken);

        if (currency is null)
        {
            logger.LogWarning("Currency not found. CurrencyCode: {CurrencyCode}", request.CurrencyCode);
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

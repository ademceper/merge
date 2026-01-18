using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.ConvertPrice;

public class ConvertPriceCommandHandler(
    IDbContext context,
        IMediator mediator,
    ILogger<ConvertPriceCommandHandler> logger) : IRequestHandler<ConvertPriceCommand, ConvertedPriceDto>
{
    public async Task<ConvertedPriceDto> Handle(ConvertPriceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Converting price. Amount: {Amount}, From: {FromCurrency}, To: {ToCurrency}", 
            request.Amount, request.FromCurrency, request.ToCurrency);

        var from = await context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Code, request.FromCurrency) && c.IsActive, cancellationToken);

        var to = await context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Code, request.ToCurrency) && c.IsActive, cancellationToken);

        if (from is null || to is null)
        {
            logger.LogWarning("Invalid currency code. From: {FromCurrency}, To: {ToCurrency}", 
                request.FromCurrency, request.ToCurrency);
            throw new ValidationException("Geçersiz para birimi kodu.");
        }

        // Convert through base currency
        // amount in base currency = amount / fromRate
        // amount in target currency = amount in base * toRate
        var baseAmount = request.Amount / from.ExchangeRate;
        var convertedAmount = baseAmount * to.ExchangeRate;

        // Round to target currency decimal places
        convertedAmount = Math.Round(convertedAmount, to.DecimalPlaces);

        // Format price
        var formatQuery = new Queries.FormatPrice.FormatPriceQuery(convertedAmount, to.Code);
        var formatted = await mediator.Send(formatQuery, cancellationToken);

        return new ConvertedPriceDto(
            OriginalAmount: request.Amount,
            FromCurrency: from.Code,
            ConvertedAmount: convertedAmount,
            ToCurrency: to.Code,
            FormattedPrice: formatted,
            ExchangeRate: to.ExchangeRate / from.ExchangeRate);
    }
}

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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class ConvertPriceCommandHandler : IRequestHandler<ConvertPriceCommand, ConvertedPriceDto>
{
    private readonly IDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<ConvertPriceCommandHandler> _logger;

    public ConvertPriceCommandHandler(
        IDbContext context,
        IMediator mediator,
        ILogger<ConvertPriceCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<ConvertedPriceDto> Handle(ConvertPriceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Converting price. Amount: {Amount}, From: {FromCurrency}, To: {ToCurrency}", 
            request.Amount, request.FromCurrency, request.ToCurrency);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var from = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.FromCurrency.ToUpper() && c.IsActive, cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var to = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.ToCurrency.ToUpper() && c.IsActive, cancellationToken);

        if (from == null || to == null)
        {
            _logger.LogWarning("Invalid currency code. From: {FromCurrency}, To: {ToCurrency}", 
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
        var formatted = await _mediator.Send(formatQuery, cancellationToken);

        return new ConvertedPriceDto(
            OriginalAmount: request.Amount,
            FromCurrency: from.Code,
            ConvertedAmount: convertedAmount,
            ToCurrency: to.Code,
            FormattedPrice: formatted,
            ExchangeRate: to.ExchangeRate / from.ExchangeRate);
    }
}


using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.PatchCurrency;

/// <summary>
/// Handler for PatchCurrencyCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchCurrencyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchCurrencyCommandHandler> logger) : IRequestHandler<PatchCurrencyCommand, CurrencyDto>
{
    public async Task<CurrencyDto> Handle(PatchCurrencyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching currency. CurrencyId: {CurrencyId}", request.Id);

        var currency = await context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (currency == null)
        {
            logger.LogWarning("Currency not found. CurrencyId: {CurrencyId}", request.Id);
            throw new NotFoundException("Para birimi", request.Id);
        }

        // Apply partial updates
        if (request.PatchDto.Name != null || request.PatchDto.Symbol != null || request.PatchDto.DecimalPlaces.HasValue || request.PatchDto.Format != null)
        {
            var name = request.PatchDto.Name ?? currency.Name;
            var symbol = request.PatchDto.Symbol ?? currency.Symbol;
            var decimalPlaces = request.PatchDto.DecimalPlaces ?? currency.DecimalPlaces;
            var format = request.PatchDto.Format ?? currency.Format;
            currency.UpdateDetails(name, symbol, decimalPlaces, format);
        }
        
        // Exchange rate güncellemesi
        if (request.PatchDto.ExchangeRate.HasValue && request.PatchDto.ExchangeRate.Value != currency.ExchangeRate)
        {
            currency.UpdateExchangeRate(request.PatchDto.ExchangeRate.Value, "PATCH");
        }
        
        // IsActive durumunu güncelle
        if (request.PatchDto.IsActive.HasValue)
        {
            if (request.PatchDto.IsActive.Value && !currency.IsActive)
            {
                currency.Activate();
            }
            else if (!request.PatchDto.IsActive.Value && currency.IsActive)
            {
                currency.Deactivate();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Currency patched successfully. CurrencyId: {CurrencyId}, Code: {Code}", currency.Id, currency.Code);

        return mapper.Map<CurrencyDto>(currency);
    }
}

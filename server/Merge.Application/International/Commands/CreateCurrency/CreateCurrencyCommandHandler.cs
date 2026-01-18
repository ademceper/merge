using MediatR;
using AutoMapper;
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

namespace Merge.Application.International.Commands.CreateCurrency;

public class CreateCurrencyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateCurrencyCommandHandler> logger) : IRequestHandler<CreateCurrencyCommand, CurrencyDto>
{
    public async Task<CurrencyDto> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating currency. Code: {Code}, Name: {Name}", request.Code, request.Name);

        var exists = await context.Set<Currency>()
            .AnyAsync(c => EF.Functions.ILike(c.Code, request.Code), cancellationToken);

        if (exists)
        {
            logger.LogWarning("Currency code already exists. Code: {Code}", request.Code);
            throw new BusinessException($"Bu para birimi kodu zaten mevcut: {request.Code}");
        }

        if (request.IsBaseCurrency)
        {
            var currentBase = await context.Set<Currency>()
                .FirstOrDefaultAsync(c => c.IsBaseCurrency, cancellationToken);

            if (currentBase != null)
            {
                currentBase.RemoveBaseCurrencyStatus();
            }
        }

        var currency = Currency.Create(
            request.Code,
            request.Name,
            request.Symbol,
            request.ExchangeRate,
            request.IsBaseCurrency,
            request.IsActive,
            request.DecimalPlaces,
            request.Format);

        await context.Set<Currency>().AddAsync(currency, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Currency created successfully. CurrencyId: {CurrencyId}, Code: {Code}", currency.Id, currency.Code);

        return mapper.Map<CurrencyDto>(currency);
    }
}

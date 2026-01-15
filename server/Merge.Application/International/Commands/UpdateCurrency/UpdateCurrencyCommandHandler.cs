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

namespace Merge.Application.International.Commands.UpdateCurrency;

public class UpdateCurrencyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateCurrencyCommandHandler> logger) : IRequestHandler<UpdateCurrencyCommand, CurrencyDto>
{
    public async Task<CurrencyDto> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating currency. CurrencyId: {CurrencyId}", request.Id);

        var currency = await context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (currency == null)
        {
            logger.LogWarning("Currency not found. CurrencyId: {CurrencyId}", request.Id);
            throw new NotFoundException("Para birimi", request.Id);
        }

        currency.UpdateDetails(request.Name, request.Symbol, request.DecimalPlaces, request.Format);
        
        // Exchange rate güncellemesi ayrı bir command olmalı, burada sadece details güncelleniyor
        // Eğer exchange rate değiştiyse, UpdateExchangeRateCommand kullanılmalı
        
        if (request.IsActive && !currency.IsActive)
        {
            currency.Activate();
        }
        else if (!request.IsActive && currency.IsActive)
        {
            currency.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Currency updated successfully. CurrencyId: {CurrencyId}, Code: {Code}", currency.Id, currency.Code);

        return mapper.Map<CurrencyDto>(currency);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.UpdateExchangeRate;

public class UpdateExchangeRateCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateExchangeRateCommandHandler> logger) : IRequestHandler<UpdateExchangeRateCommand, Unit>
{
    public async Task<Unit> Handle(UpdateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating exchange rate. CurrencyCode: {CurrencyCode}, NewRate: {NewRate}, Source: {Source}", 
            request.CurrencyCode, request.NewRate, request.Source);

        var currency = await context.Set<Currency>()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Code, request.CurrencyCode), cancellationToken);

        if (currency is null)
        {
            logger.LogWarning("Currency not found. CurrencyCode: {CurrencyCode}", request.CurrencyCode);
            throw new NotFoundException("Para birimi", Guid.Empty);
        }

        currency.UpdateExchangeRate(request.NewRate, request.Source);

        // Save to history
        var history = ExchangeRateHistory.Create(
            currency.Id,
            currency.Code,
            request.NewRate,
            request.Source);

        await context.Set<ExchangeRateHistory>().AddAsync(history, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Exchange rate updated successfully. CurrencyCode: {CurrencyCode}, NewRate: {NewRate}", 
            request.CurrencyCode, request.NewRate);
        return Unit.Value;
    }
}

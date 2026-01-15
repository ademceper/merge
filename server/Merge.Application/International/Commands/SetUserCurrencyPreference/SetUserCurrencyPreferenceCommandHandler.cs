using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.SetUserCurrencyPreference;

public class SetUserCurrencyPreferenceCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<SetUserCurrencyPreferenceCommandHandler> logger) : IRequestHandler<SetUserCurrencyPreferenceCommand, Unit>
{
    public async Task<Unit> Handle(SetUserCurrencyPreferenceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Setting user currency preference. UserId: {UserId}, CurrencyCode: {CurrencyCode}", 
            request.UserId, request.CurrencyCode);

        var currency = await context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.CurrencyCode.ToUpper() && c.IsActive, cancellationToken);

        if (currency == null)
        {
            logger.LogWarning("Currency not found. CurrencyCode: {CurrencyCode}", request.CurrencyCode);
            throw new NotFoundException("Para birimi", Guid.Empty);
        }

        var preference = await context.Set<UserCurrencyPreference>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (preference == null)
        {
            preference = UserCurrencyPreference.Create(
                request.UserId,
                currency.Id,
                currency.Code);
            await context.Set<UserCurrencyPreference>().AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.UpdateCurrency(currency.Id, currency.Code);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User currency preference set successfully. UserId: {UserId}, CurrencyCode: {CurrencyCode}", 
            request.UserId, request.CurrencyCode);
        return Unit.Value;
    }
}

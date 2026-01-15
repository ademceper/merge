using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.DeleteCurrency;

public class DeleteCurrencyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCurrencyCommandHandler> logger) : IRequestHandler<DeleteCurrencyCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting currency. CurrencyId: {CurrencyId}", request.Id);

        var currency = await context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (currency == null)
        {
            logger.LogWarning("Currency not found. CurrencyId: {CurrencyId}", request.Id);
            throw new NotFoundException("Para birimi", request.Id);
        }

        currency.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Currency deleted successfully. CurrencyId: {CurrencyId}, Code: {Code}", currency.Id, currency.Code);
        return Unit.Value;
    }
}

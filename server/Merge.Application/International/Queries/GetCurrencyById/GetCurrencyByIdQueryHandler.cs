using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetCurrencyById;

public class GetCurrencyByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCurrencyByIdQueryHandler> logger) : IRequestHandler<GetCurrencyByIdQuery, CurrencyDto?>
{
    public async Task<CurrencyDto?> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting currency by ID. CurrencyId: {CurrencyId}", request.Id);

        var currency = await context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return currency is not null ? mapper.Map<CurrencyDto>(currency) : null;
    }
}

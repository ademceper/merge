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

namespace Merge.Application.International.Queries.GetActiveCurrencies;

public class GetActiveCurrenciesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetActiveCurrenciesQueryHandler> logger) : IRequestHandler<GetActiveCurrenciesQuery, IEnumerable<CurrencyDto>>
{
    public async Task<IEnumerable<CurrencyDto>> Handle(GetActiveCurrenciesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting active currencies");

        var currencies = await context.Set<Currency>()
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .Take(200)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<CurrencyDto>>(currencies);
    }
}

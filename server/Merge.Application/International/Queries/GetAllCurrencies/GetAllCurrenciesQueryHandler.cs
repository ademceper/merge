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

namespace Merge.Application.International.Queries.GetAllCurrencies;

public class GetAllCurrenciesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllCurrenciesQueryHandler> logger) : IRequestHandler<GetAllCurrenciesQuery, IEnumerable<CurrencyDto>>
{
    public async Task<IEnumerable<CurrencyDto>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all currencies");

        var currencies = await context.Set<Currency>()
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Take(500)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<CurrencyDto>>(currencies);
    }
}

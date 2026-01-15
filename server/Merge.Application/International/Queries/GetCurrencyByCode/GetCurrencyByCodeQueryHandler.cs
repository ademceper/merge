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

namespace Merge.Application.International.Queries.GetCurrencyByCode;

public class GetCurrencyByCodeQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCurrencyByCodeQueryHandler> logger) : IRequestHandler<GetCurrencyByCodeQuery, CurrencyDto?>
{
    public async Task<CurrencyDto?> Handle(GetCurrencyByCodeQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting currency by code. Code: {Code}", request.Code);

        var currency = await context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.Code.ToUpper(), cancellationToken);

        return currency != null ? mapper.Map<CurrencyDto>(currency) : null;
    }
}

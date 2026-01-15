using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetExchangeRateHistory;

public class GetExchangeRateHistoryQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetExchangeRateHistoryQueryHandler> logger) : IRequestHandler<GetExchangeRateHistoryQuery, IEnumerable<ExchangeRateHistoryDto>>
{
    public async Task<IEnumerable<ExchangeRateHistoryDto>> Handle(GetExchangeRateHistoryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting exchange rate history. CurrencyCode: {CurrencyCode}, Days: {Days}", 
            request.CurrencyCode, request.Days);

        var startDate = DateTime.UtcNow.AddDays(-request.Days);

        var history = await context.Set<ExchangeRateHistory>()
            .AsNoTracking()
            .Where(h => h.CurrencyCode.ToUpper() == request.CurrencyCode.ToUpper() &&
                       h.RecordedAt >= startDate)
            .OrderByDescending(h => h.RecordedAt)
            .Take(1000)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<ExchangeRateHistoryDto>>(history);
    }
}

using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetExchangeRateHistory;

public record GetExchangeRateHistoryQuery(
    string CurrencyCode,
    int Days = 30) : IRequest<IEnumerable<ExchangeRateHistoryDto>>;


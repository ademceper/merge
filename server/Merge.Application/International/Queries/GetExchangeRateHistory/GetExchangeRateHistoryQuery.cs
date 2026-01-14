using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetExchangeRateHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetExchangeRateHistoryQuery(
    string CurrencyCode,
    int Days = 30) : IRequest<IEnumerable<ExchangeRateHistoryDto>>;


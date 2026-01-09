using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetCurrencyStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCurrencyStatsQuery() : IRequest<CurrencyStatsDto>;


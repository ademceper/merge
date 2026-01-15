using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetCurrencyStats;

public record GetCurrencyStatsQuery() : IRequest<CurrencyStatsDto>;


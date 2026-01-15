using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetActiveCurrencies;

public record GetActiveCurrenciesQuery() : IRequest<IEnumerable<CurrencyDto>>;


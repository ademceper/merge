using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetAllCurrencies;

public record GetAllCurrenciesQuery() : IRequest<IEnumerable<CurrencyDto>>;


using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetActiveCurrencies;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetActiveCurrenciesQuery() : IRequest<IEnumerable<CurrencyDto>>;


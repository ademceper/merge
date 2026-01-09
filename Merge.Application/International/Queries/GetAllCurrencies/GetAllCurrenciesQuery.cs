using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetAllCurrencies;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllCurrenciesQuery() : IRequest<IEnumerable<CurrencyDto>>;


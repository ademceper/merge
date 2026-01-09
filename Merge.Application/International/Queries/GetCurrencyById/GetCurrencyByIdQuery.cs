using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetCurrencyById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCurrencyByIdQuery(Guid Id) : IRequest<CurrencyDto?>;


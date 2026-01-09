using MediatR;

namespace Merge.Application.International.Queries.FormatPrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record FormatPriceQuery(
    decimal Amount,
    string CurrencyCode) : IRequest<string>;


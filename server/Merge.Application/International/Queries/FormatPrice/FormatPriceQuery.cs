using MediatR;

namespace Merge.Application.International.Queries.FormatPrice;

public record FormatPriceQuery(
    decimal Amount,
    string CurrencyCode) : IRequest<string>;


using MediatR;

namespace Merge.Application.Marketing.Queries.CalculateGiftCardDiscount;

public record CalculateGiftCardDiscountQuery(
    string Code,
    decimal OrderAmount) : IRequest<decimal>;

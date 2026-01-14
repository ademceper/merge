using MediatR;

namespace Merge.Application.Marketing.Queries.CalculateDiscountFromPoints;

public record CalculateDiscountFromPointsQuery(
    int Points) : IRequest<decimal>;

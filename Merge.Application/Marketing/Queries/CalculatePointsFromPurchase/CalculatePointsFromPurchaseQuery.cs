using MediatR;

namespace Merge.Application.Marketing.Queries.CalculatePointsFromPurchase;

public record CalculatePointsFromPurchaseQuery(
    decimal Amount) : IRequest<int>;

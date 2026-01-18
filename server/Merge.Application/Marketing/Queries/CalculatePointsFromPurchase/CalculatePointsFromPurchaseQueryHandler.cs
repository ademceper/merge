using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Marketing.Queries.CalculatePointsFromPurchase;

public class CalculatePointsFromPurchaseQueryHandler(IOptions<LoyaltySettings> loyaltySettings) : IRequestHandler<CalculatePointsFromPurchaseQuery, int>
{
    public Task<int> Handle(CalculatePointsFromPurchaseQuery request, CancellationToken cancellationToken)
    {
        var pointsRate = loyaltySettings.Value.PointsPerCurrencyUnit ?? 1.0m; // Default: $1 = 1 point
        return Task.FromResult((int)(request.Amount * pointsRate));
    }
}

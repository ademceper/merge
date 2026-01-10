using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Marketing.Queries.CalculatePointsFromPurchase;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CalculatePointsFromPurchaseQueryHandler : IRequestHandler<CalculatePointsFromPurchaseQuery, int>
{
    private readonly LoyaltySettings _loyaltySettings;

    public CalculatePointsFromPurchaseQueryHandler(IOptions<LoyaltySettings> loyaltySettings)
    {
        _loyaltySettings = loyaltySettings.Value;
    }

    public Task<int> Handle(CalculatePointsFromPurchaseQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var pointsRate = _loyaltySettings.PointsPerCurrencyUnit ?? 1.0m; // Default: $1 = 1 point
        return Task.FromResult((int)(request.Amount * pointsRate));
    }
}

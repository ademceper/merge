using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Marketing.Queries.CalculatePointsFromPurchase;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CalculatePointsFromPurchaseQueryHandler(IOptions<LoyaltySettings> loyaltySettings) : IRequestHandler<CalculatePointsFromPurchaseQuery, int>
{
    public Task<int> Handle(CalculatePointsFromPurchaseQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var pointsRate = loyaltySettings.Value.PointsPerCurrencyUnit ?? 1.0m; // Default: $1 = 1 point
        return Task.FromResult((int)(request.Amount * pointsRate));
    }
}

using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Marketing.Queries.CalculateDiscountFromPoints;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CalculateDiscountFromPointsQueryHandler(IOptions<LoyaltySettings> loyaltySettings) : IRequestHandler<CalculateDiscountFromPointsQuery, decimal>
{
    public Task<decimal> Handle(CalculateDiscountFromPointsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var discountRate = loyaltySettings.Value.CurrencyPerPoint ?? 0.01m; // Default: 1 point = $0.01
        return Task.FromResult(request.Points * discountRate);
    }
}

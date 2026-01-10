using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Marketing.Queries.CalculateDiscountFromPoints;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CalculateDiscountFromPointsQueryHandler : IRequestHandler<CalculateDiscountFromPointsQuery, decimal>
{
    private readonly LoyaltySettings _loyaltySettings;

    public CalculateDiscountFromPointsQueryHandler(IOptions<LoyaltySettings> loyaltySettings)
    {
        _loyaltySettings = loyaltySettings.Value;
    }

    public Task<decimal> Handle(CalculateDiscountFromPointsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var discountRate = _loyaltySettings.CurrencyPerPoint ?? 0.01m; // Default: 1 point = $0.01
        return Task.FromResult(request.Points * discountRate);
    }
}

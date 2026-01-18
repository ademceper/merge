using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Marketing.Queries.CalculateDiscountFromPoints;

public class CalculateDiscountFromPointsQueryHandler(IOptions<LoyaltySettings> loyaltySettings) : IRequestHandler<CalculateDiscountFromPointsQuery, decimal>
{
    public Task<decimal> Handle(CalculateDiscountFromPointsQuery request, CancellationToken cancellationToken)
    {
        var discountRate = loyaltySettings.Value.CurrencyPerPoint ?? 0.01m; // Default: 1 point = $0.01
        return Task.FromResult(request.Points * discountRate);
    }
}

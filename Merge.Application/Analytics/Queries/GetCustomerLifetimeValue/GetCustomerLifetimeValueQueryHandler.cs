using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetCustomerLifetimeValue;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCustomerLifetimeValueQueryHandler : IRequestHandler<GetCustomerLifetimeValueQuery, CustomerLifetimeValueDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GetCustomerLifetimeValueQueryHandler> _logger;

    public GetCustomerLifetimeValueQueryHandler(
        IAnalyticsService analyticsService,
        ILogger<GetCustomerLifetimeValueQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<CustomerLifetimeValueDto> Handle(GetCustomerLifetimeValueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching customer lifetime value. CustomerId: {CustomerId}", request.CustomerId);

        var ltv = await _analyticsService.GetCustomerLifetimeValueAsync(request.CustomerId, cancellationToken);

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        return new CustomerLifetimeValueDto(request.CustomerId, ltv);
    }
}


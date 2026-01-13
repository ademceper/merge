using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Configuration;
using Merge.Domain.Interfaces;

namespace Merge.Application.Logistics.Queries.GetAvailableShippingProviders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetAvailableShippingProvidersQueryHandler : IRequestHandler<GetAvailableShippingProvidersQuery, IEnumerable<ShippingProviderDto>>
{
    private readonly ILogger<GetAvailableShippingProvidersQueryHandler> _logger;
    private readonly ShippingSettings _shippingSettings;

    public GetAvailableShippingProvidersQueryHandler(
        ILogger<GetAvailableShippingProvidersQueryHandler> logger,
        IOptions<ShippingSettings> shippingSettings)
    {
        _logger = logger;
        _shippingSettings = shippingSettings.Value;
    }

    public Task<IEnumerable<ShippingProviderDto>> Handle(GetAvailableShippingProvidersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting available shipping providers");

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        var providers = _shippingSettings.Providers
            .Select(kvp => new ShippingProviderDto(
                kvp.Key,
                kvp.Value.Name,
                kvp.Value.BaseCost,
                kvp.Value.EstimatedDays))
            .ToList();

        return Task.FromResult<IEnumerable<ShippingProviderDto>>(providers);
    }
}


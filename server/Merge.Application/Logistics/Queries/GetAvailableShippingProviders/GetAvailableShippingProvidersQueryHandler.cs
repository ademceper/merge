using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Configuration;
using Merge.Domain.Interfaces;

namespace Merge.Application.Logistics.Queries.GetAvailableShippingProviders;

public class GetAvailableShippingProvidersQueryHandler(
    ILogger<GetAvailableShippingProvidersQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetAvailableShippingProvidersQuery, IEnumerable<ShippingProviderDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public Task<IEnumerable<ShippingProviderDto>> Handle(GetAvailableShippingProvidersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting available shipping providers");

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


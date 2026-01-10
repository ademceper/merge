using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetAvailableShippingProviders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetAvailableShippingProvidersQueryHandler : IRequestHandler<GetAvailableShippingProvidersQuery, IEnumerable<ShippingProviderDto>>
{
    private readonly ILogger<GetAvailableShippingProvidersQueryHandler> _logger;

    public GetAvailableShippingProvidersQueryHandler(
        ILogger<GetAvailableShippingProvidersQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<ShippingProviderDto>> Handle(GetAvailableShippingProvidersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting available shipping providers");

        // Gerçek uygulamada veritabanından veya config'den alınacak
        var providers = new List<ShippingProviderDto>
        {
            new ShippingProviderDto { Code = "YURTICI", Name = "Yurtiçi Kargo", BaseCost = 50m, EstimatedDays = 3 },
            new ShippingProviderDto { Code = "ARAS", Name = "Aras Kargo", BaseCost = 45m, EstimatedDays = 2 },
            new ShippingProviderDto { Code = "MNG", Name = "MNG Kargo", BaseCost = 40m, EstimatedDays = 2 },
            new ShippingProviderDto { Code = "SURAT", Name = "Sürat Kargo", BaseCost = 55m, EstimatedDays = 3 }
        };

        return Task.FromResult<IEnumerable<ShippingProviderDto>>(providers);
    }
}


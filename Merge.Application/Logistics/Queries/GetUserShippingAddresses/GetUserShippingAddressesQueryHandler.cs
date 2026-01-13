using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetUserShippingAddresses;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetUserShippingAddressesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetUserShippingAddressesQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetUserShippingAddressesQuery, IEnumerable<ShippingAddressDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<IEnumerable<ShippingAddressDto>> Handle(GetUserShippingAddressesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting user shipping addresses. UserId: {UserId}, IsActive: {IsActive}", request.UserId, request.IsActive);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var query = context.Set<ShippingAddress>()
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId);

        if (request.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == request.IsActive.Value);
        }

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        var addresses = await query
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Label)
            .Take(_shippingSettings.QueryLimits.MaxShippingAddressesPerUser)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return mapper.Map<IEnumerable<ShippingAddressDto>>(addresses);
    }
}


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
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetAllWarehouses;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetAllWarehousesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllWarehousesQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetAllWarehousesQuery, IEnumerable<WarehouseDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<IEnumerable<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all warehouses. IncludeInactive: {IncludeInactive}", request.IncludeInactive);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var query = context.Set<Warehouse>().AsNoTracking();

        if (!request.IncludeInactive)
        {
            query = query.Where(w => w.IsActive);
        }

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        var warehouses = await query
            .OrderBy(w => w.Name)
            .Take(_shippingSettings.QueryLimits.MaxWarehouses)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }
}


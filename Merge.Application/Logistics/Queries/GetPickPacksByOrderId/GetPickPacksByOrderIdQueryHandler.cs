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
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetPickPacksByOrderId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetPickPacksByOrderIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetPickPacksByOrderIdQueryHandler> logger,
    IOptions<ShippingSettings> shippingSettings) : IRequestHandler<GetPickPacksByOrderIdQuery, IEnumerable<PickPackDto>>
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    public async Task<IEnumerable<PickPackDto>> Handle(GetPickPacksByOrderIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting pick-packs by order. OrderId: {OrderId}", request.OrderId);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için cartesian explosion önleme
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        var pickPacks = await context.Set<PickPack>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting (AsSplitQuery) - Cartesian explosion önleme
            .Include(pp => pp.Order)
            .Include(pp => pp.Warehouse)
            .Include(pp => pp.PickedBy)
            .Include(pp => pp.PackedBy)
            .Include(pp => pp.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .Where(pp => pp.OrderId == request.OrderId)
            .OrderByDescending(pp => pp.CreatedAt)
            .Take(_shippingSettings.QueryLimits.MaxPickPacksPerOrder)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return mapper.Map<IEnumerable<PickPackDto>>(pickPacks);
    }
}


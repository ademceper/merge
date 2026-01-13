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

namespace Merge.Application.Logistics.Queries.GetActiveWarehouses;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetActiveWarehousesQueryHandler : IRequestHandler<GetActiveWarehousesQuery, IEnumerable<WarehouseDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActiveWarehousesQueryHandler> _logger;
    private readonly ShippingSettings _shippingSettings;

    public GetActiveWarehousesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetActiveWarehousesQueryHandler> logger,
        IOptions<ShippingSettings> shippingSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _shippingSettings = shippingSettings.Value;
    }

    public async Task<IEnumerable<WarehouseDto>> Handle(GetActiveWarehousesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting active warehouses");

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        var warehouses = await _context.Set<Warehouse>()
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .Take(_shippingSettings.QueryLimits.MaxWarehouses)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }
}


using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
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

    public GetActiveWarehousesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetActiveWarehousesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<WarehouseDto>> Handle(GetActiveWarehousesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting active warehouses");

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var warehouses = await _context.Set<Warehouse>()
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }
}


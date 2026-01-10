using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Queries.GetAllWarehouses;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, IEnumerable<WarehouseDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllWarehousesQueryHandler> _logger;

    public GetAllWarehousesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllWarehousesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all warehouses. IncludeInactive: {IncludeInactive}", request.IncludeInactive);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var query = _context.Set<Warehouse>().AsNoTracking();

        if (!request.IncludeInactive)
        {
            query = query.Where(w => w.IsActive);
        }

        var warehouses = await query
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }
}


using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByInventoryId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetStockMovementsByInventoryIdQueryHandler : IRequestHandler<GetStockMovementsByInventoryIdQuery, IEnumerable<StockMovementDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetStockMovementsByInventoryIdQueryHandler> _logger;

    public GetStockMovementsByInventoryIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetStockMovementsByInventoryIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<StockMovementDto>> Handle(GetStockMovementsByInventoryIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting stock movements by inventory. InventoryId: {InventoryId}", request.InventoryId);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var movements = await _context.Set<StockMovement>()
            .AsNoTracking()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .Include(sm => sm.User)
            .Include(sm => sm.FromWarehouse)
            .Include(sm => sm.ToWarehouse)
            .Where(sm => sm.InventoryId == request.InventoryId)
            .OrderByDescending(sm => sm.CreatedAt)
            .Take(100) // ✅ Güvenlik: Maksimum 100 hareket
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
    }
}

